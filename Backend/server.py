from flask import Flask, request, jsonify
from Agents.trait_picker import get_assigned_traits
from Agents.Agent import generate_agent, generate_micro
from Agents.Manager import GameManager as GM
from SQL import init_sqlite
from os import environ
from dotenv import load_dotenv
import pdb

app = Flask(__name__)

@app.route('/init_game', methods=['POST'])
def init_game():
    json_data = request.get_json()
    user_id = json_data.get('user_id', '')
    exe, com = init_sqlite("gamestate.db")
    GM.GM_init(user_id, exe, com)
    GM.load_static_tables(json_data)
    return jsonify({"message": "Game initialized"}), 200

@app.route('/init_test', methods=['POST'])
def init_test():
    json_data = request.get_json()
    user_id = json_data.get('user_id', '')
    exe, com = init_sqlite(json_data.get('db_name', ''))
    GM.GM_init(user_id, exe, com)
    GM.load_static_tables(json_data)
    return jsonify({"message": "Game initialized"}), 200

@app.route('/init_turn', methods=['POST'])
def init_turn():
    json_data = request.get_json()
    GM.cur("DELETE FROM Units")
    GM.cur("DELETE FROM Equipment")
    GM.load_tables(json_data)
    GM.decision_dict = {}
    return jsonify({"message": "Turn initialized"}), 200

@app.route('/init_micro', methods=['POST'])
def init_micro():
    # pdb.set_trace()
    if not GM:
        return jsonify({"error": "Game not initialized"}), 400
    json_data = request.get_json()
    ally_data = json_data.get('unit', {})
    enemy_data = json_data.get('enemy', {})
    class CommanderInfo:
        assistant_id = None
        spell_request = None
        battalion_command = None
        command_group = 0
        format_spell = lambda self, spell_type, spell_info: {"spell_type":spell_type, "spell_info":spell_info}
        format_command = lambda self, command_type, command_group: {"command_type":command_type, "command_group":command_group}
        def __init__(self):
            self.spell_request = {"spell_type":None, "spell_info":None}
            self.battalion_command = {"0":None, "1":None, "2":None}

            agent_list = GM.assistants.list()
            for agent in agent_list:
                if agent.name == "Battalion Commander":
                    self.assistant_id = agent.id
                    break
            if not self.assistant_id:
                from Microbattle.Sysmsg_Helper import sys_dict
                from Microbattle.Commander import tool_arr
                self.assistant_id = GM.assistants.create(
                    instructions=f"{sys_dict['header']}\n{sys_dict['micro_commander']}",
                    name="Battalion Commander",
                    tools=tool_arr,
                    model="gpt-4o-mini"
                ).id

        def setspell(self, spell_type, spell_info):
            # pdb.set_trace()
            self.spell_request = self.format_spell(spell_type=spell_type, spell_info=spell_info)
        
        def setcommand(self, command_type):
            print("setcommand")
            if self.command_group < 3:
                self.battalion_command[f"{self.command_group}"] = command_type
                self.command_group += 1
                return f"order sent to command group: {self.command_group}"
            if self.spell_request:
                return None
            return "Command group full, DO NOT CALL BATTALION_COMMAND AGAIN, only spell_request will be accepted"
        
        def getcomdecision(self):
            print("getdecision")
            # pdb.set_trace()
            return {"spell_request":self.spell_request, "battalion_command":self.battalion_command}
    class UnitInfo:
        hp = None
        x_coord = None
        z_coord = None
        decision = None
        battalion = None
        target_selection = None
        def __init__(self, unit_id):
            self.unit_id = unit_id
            self.thread_id = GM.threads.create().id

        def load_schema(self, info):
            self.hp = info["hp"]
            self.x_coord = info["x_coord"]
            self.z_coord = info["z_coord"]
            self.battalion = info["battalion"]
            self.decision = None
            self.target_selection = dict()

        def getpos(self):
            return self.x_coord, self.z_coord
        
        def setpos(self, x_coord, z_coord):
            self.x_coord = x_coord
            self.z_coord = z_coord

        def getdecision(self):
            def last(decision_type):

                if type(self.decision) == dict:
                    # pdb.set_trace()
                    if self.decision["decision_type"] == decision_type:
                        return self.decision
                    return None
            
                i = len(self.decision) - 1
                while i >= 0:
                    if self.decision[i]["decision_type"] == decision_type:
                        return self.decision[i]
                    i = i - 1
                return None
            
            move = None
            target = None
            if self.decision:
                move = last("move")
                target = last("target")

            formatdec = lambda decision_type, decision_info: {"decision_type": decision_type, "decision_info": decision_info}
            if not move:
                move = formatdec("move", f"{self.x_coord+1},{self.z_coord+1}")
                target = None
            target = formatdec("target", None) if not target else target
            
            return [move, target]

    GM.micro_unit = UnitInfo(ally_data["id"])
    GM.micro_enemy = UnitInfo(enemy_data["id"])
    GM.micro_commander = CommanderInfo()

    GM.load_micro(ally_data, enemy_data)
    GM.load_micro(enemy_data, ally_data)

    return jsonify({"message":"micro init"}), 200


@app.route('/uninit_micro', methods=['POST'])
def uninit_micro():
    if not GM:
        return jsonify({"error": "Game not initialized"}), 400
    json_data = request.get_json()
    GM.load_map(json_data.get('winner_id', ''))
    GM.assistants.delete(json_data.get('loser_id', ''))
    return jsonify({"message": "Microbattle ended"}), 200

@app.route('/create_agent', methods=['POST'])
def create_agent():
    if not GM:
        return jsonify({"error": "Game not initialized"}), 400
    json_data = request.get_json()
    agent_name = json_data.get('agent_name', '')
    agent_description = json_data.get('agent_description', '')
    agent_class = json_data.get('agent_class', '')

    if not agent_name or not agent_description:
        return jsonify({"error": "Invalid input, please provide agent_name and agent_description"}), 400

    unit_traits = get_assigned_traits(agent_description)
    unit_id = generate_agent(agent_description, agent_name, unit_traits, agent_class)

    response_data = {
        "unit_id": unit_id,
        "thread_id": GM.threads.create().id,
        "assigned_traits": unit_traits
    }

    return jsonify(response_data), 200

@app.route('/destroy_agent', methods=['POST'])
def destroy_agent():
    if not GM:
        return jsonify({"error": "Game not initialized"}), 400
    json_data = request.get_json()
    agent_id = json_data.get('agent_id', '')

    if not agent_id:
        return jsonify({"error": "Invalid input, please provide agent_id"}), 400

    GM.assistants.delete(agent_id)

    return jsonify({"success": "agent deleted"}), 200

@app.route('/invoke_micro', methods=['POST'])
def invoke_micro():
    if not GM:
        return jsonify({"error": "Game not initialized"}), 400
    json_data = request.get_json()
    unit_info = json_data.get('unit', '')
    enemy_info = json_data.get('enemy', '')
    # pdb.set_trace()

    GM.micro_unit.load_schema(unit_info)
    GM.micro_enemy.load_schema(enemy_info)

    GM.invoke_micro(json_data["invoke"], json_data["orders"])
    ret_info, buffer = GM.yield_agents(json_data["invoke"])

    return jsonify(ret_info.getdecision()), 200

@app.route('/invoke_agent', methods=['POST'])
def invoke_agent():
    try:
        GM.user_id
    except:
        return jsonify({"error": "Game not initialized"}), 400
    json_data = request.get_json()
    
    agent_id = json_data.get('agent_id', '')

    if not agent_id:
        return jsonify({"error": "Invalid input, please provide agent_id"}), 400

    # pdb.set_trace()
    GM.invoke_agent(agent_id)
    # pdb.set_trace()

    return jsonify({"decision": GM.decision_dict[agent_id], "messages": GM.sent_dict[agent_id]}), 200

@app.route('/invoke_commander', methods=['POST'])
def invoke_commander():
    try:
        GM.user_id
    except:
        return jsonify({"error": "Game not initialized"}), 400
    
    if not GM.micro_unit.battalion or not GM.micro_enemy.battalion:
        return jsonify({"error": "have both units take their turn first"})

    from Microbattle.Commander import MicrobattleHandler
    try:
        with GM.runs.stream(
            assistant_id=GM.micro_commander.assistant_id,
            thread_id=GM.threads.create().id,
            additional_messages=[{"role": "user", "content": f"Friendly Battalion:\n{GM.micro_unit.battalion}\n\nEnemy Battalion:\n{GM.micro_enemy.battalion}"}],
            event_handler=MicrobattleHandler(),
            parallel_tool_calls=False
        ) as stream:
            stream.until_done()
    except Exception as e:
        print(e)
    return jsonify(GM.micro_commander.getcomdecision()), 200




@app.route('/bomba', methods=['GET'])
def nuclear_bomba():
    cursor, commit = init_sqlite("gamestate.db")
    GM.GM_init(0, cursor, commit)
    GM.cur("DROP TABLE IF EXISTS Units")
    GM.cur("DROP TABLE IF EXISTS Resources")
    agent_list = GM.assistants.list()
    try:
        for agent in agent_list:
            try:
                GM.assistants.delete(agent.id)
            except:
                continue
    except:
        pass
    return jsonify({"message": "bomba"}), 200

@app.route('/get_units', methods=['GET'])
def get_units():
    cursor, commit = init_sqlite("gamestate.db")
    GM.GM_init(0, cursor, commit)
    agents_list = GM.assistants.list()
    for agent in agents_list:
        print(f"Agent ID: {agent.id}\nAgent Name: {agent.name}\nAgent Metadata: {agent.metadata}\n")
    return jsonify({"message": "Units retrieved"}), 200

if __name__ == '__main__':
    try:
        environ.pop("OPENAI_API_KEY", None)
    finally:
        load_dotenv()
    app.run(host='0.0.0.0', port=5000)
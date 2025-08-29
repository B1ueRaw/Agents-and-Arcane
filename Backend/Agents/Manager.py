from functools import wraps
from enum import Enum
from openai import OpenAI
from threading import Lock
from json import loads
import pdb

class MetaManager(type):

    _instances = {}
    def __call__(cls, *args, **kwargs):
        if cls not in cls._instances:
            cls._instances[cls] = super().__call__(*args, **kwargs)
        return cls._instances[cls]

    def __getitem__(cls, key: str):
        return cls.lock_dict[key]
    
    # def __setitem__(cls, key: str, value):
    #     cls.agent_dict[key] = value
    
    def __getattr__(cls, key: str):
        if key == "message":
            return cls.message_dict[cls.current_agent]
        if key == "decision":
            return cls.decision_dict[cls.current_agent]
        try:
            return super().__getattr__(key)
        except AttributeError:
            if hasattr(cls.client_id.beta, key):
                return getattr(cls.client_id.beta, key)
            elif hasattr(cls.client_id.beta.threads, key):
                return getattr(cls.client_id.beta.threads, key)
        raise AttributeError(f"{cls.__name__} has no attribute {key}")
            
    def __setattr__(cls, key: str, value):
        if key == "message":
            cls.message_dict[cls.current_agent] = value
        if key == "decision":
            cls.decision_dict[cls.current_agent] = value
        try:
            super().__setattr__(key, value)
        except AttributeError:
            cls.key = value
            
            
class GameManager(object, metaclass=MetaManager):
    user_id = None
    client_id = None
    cur_id = None
    db_id = None
    lck_id = None
    inventory_dict = None
    lock_dict = None
    pid_dict = None
    message_dict = None
    micro_unit = None
    micro_enemy = None
    micro_commander = None
    decision_dict = None
    sent_dict = None

    @staticmethod
    def GM_init(user_id, cursor_id, commit):
        GameManager.user_id = user_id
        GameManager.client_id = OpenAI()
        GameManager.cur_id = cursor_id
        GameManager.db_id = commit
        GameManager.lck_id = {"cur": Lock(), "inv": Lock()}
        GameManager.inventory_dict = dict()
        GameManager.message_dict = dict()
        GameManager.decision_dict = dict()
        GameManager.sent_dict = dict()

    @staticmethod
    def cur(statement):
        with GameManager.lck_id["cur"]:
            ret = GameManager.cur_id(statement).fetchall()
            GameManager.db_id()
        return ret
    
    @staticmethod
    def yield_agents(unit_id):
        unit_info = GameManager.micro_unit if unit_id == GameManager.micro_unit.unit_id else GameManager.micro_enemy
        enemy_info = GameManager.micro_unit if unit_id != GameManager.micro_unit.unit_id else GameManager.micro_enemy
        return unit_info, enemy_info

    @staticmethod
    def load_static_tables(json_data):
        resources = json_data["resources"]
        classes = json_data["classes"]
        GameManager.cur("DELETE FROM Resources")
        GameManager.cur("DELETE FROM Classes")
        for resource in resources:
            GameManager.cur(f"INSERT INTO Resources(x_coord, y_coord, type) VALUES({resource['x_coord']}, {resource['y_coord']}, {resource['type']})")
        for class_ in classes:
            GameManager.cur(f"INSERT INTO Classes(class_id, base_hp, base_ms, ability) VALUES({class_['class_id']}, {class_['base_hp']}, {class_['base_ms']}, '{class_['ability']}')")
    
    @staticmethod
    def load_tables(json_data):
        units = json_data["units"]
        equipment = json_data["equipment"]
        GameManager.inventory_dict = json_data["inventory"]
        for unit in units:
            GameManager.message_dict[unit["agent_id"]] = None
            GameManager.decision_dict[unit["agent_id"]] = None
            GameManager.sent_dict[unit["agent_id"]] = dict()
            for recipients in units:
                if recipients["agent_id"] != unit["agent_id"]:
                    GameManager.sent_dict[unit["agent_id"]][recipients["agent_id"]] = None
            GameManager.cur(f"INSERT INTO Units(unit_id, x_coord, y_coord, thread_id, hp, class_id, equip_id, team, has_moved, orders) VALUES('{unit['agent_id']}', {unit['x_coord']}, {unit['y_coord']}, '{unit['thread_id']}', {unit['hp']}, {unit['class_id']}, {unit['equip_id']}, {unit['team']}, {unit['has_moved']}, '{unit['order']}')")
        for equip in equipment:
            GameManager.cur(f"INSERT INTO Equipment(equip_id, tier, description) VALUES({equip['equip_id']}, {equip['tier']}, '{equip['description']}')")

    @staticmethod
    def load_micro(unit_data, enemy_data):
        from Agents.Agent import generate_micro
        from Microbattle.Tools import m_tool_arr
        unit = GameManager.assistants.retrieve(unit_data["id"])
        meta_data = unit.metadata

        GameManager.assistants.update(
            unit_data["id"],
            instructions=generate_micro(loads(meta_data["traits"]), unit_data["tier"], enemy_data["tier"]),
            tools=m_tool_arr
        )

    @staticmethod
    def load_map(unit_data):
        from Agents.Agent import generate_agent
        unit = GameManager.assistants.retrieve(unit_data["id"])
        data = unit.metadata

        GameManager.assistants.update(
            unit_data["id"],
            instructions=generate_agent(data["story"], unit.name, loads(data["traits"]), data["class"])
        )

    @staticmethod
    def invoke_micro(unit_id, orders):
        unit_info, enemy_info = GameManager.yield_agents(unit_id)
        prompt= f'''orders: {orders}
        your info: [HP: {unit_info.hp}, coordinates: ({unit_info.x_coord}, {unit_info.z_coord})]
        enemy commander info: [HP: {enemy_info.hp}, coordinates: ({enemy_info.x_coord}, {enemy_info.z_coord})]
        FRIENDLY battalion info: {unit_info.battalion}
        -----------------------
        ENEMY battalion info: {enemy_info.battalion}
        '''
        from Microbattle.Tools import ToolHandlerM
        try:
            with GameManager.runs.stream(
                assistant_id=unit_id,
                thread_id=unit_info.thread_id,
                additional_messages=[{"role": "user", "content": prompt}],
                event_handler=ToolHandlerM(),
                parallel_tool_calls=False
            ) as stream:
                stream.until_done()
        except Exception as e:
            print(e)

    @staticmethod
    def invoke_agent(agent_id):
        with GameManager.lck_id["inv"]:
            GameManager.current_agent = agent_id
            GameManager.decision = None
            messages = None
            try:
                messages = GameManager.message_dict[agent_id]
            except KeyError:
                pass
            GameManager.message = None
            for key in GameManager.sent_dict[agent_id]:
                GameManager.sent_dict[agent_id][key] = None
            from Agents.Tools import ToolHandler
            agent_info = GameManager.cur(f"SELECT x_coord, y_coord, hp, tier, orders, thread_id FROM Units u LEFT JOIN Equipment e ON u.equip_id=e.equip_id WHERE unit_id='{agent_id}'")[0]
            thread_id = agent_info[5]
            if thread_id == "generate":
                thread_id = GameManager.threads.create().id
            GameManager.current_thread = thread_id
            GameManager.current_position = {"x":agent_info[0], "y":agent_info[1]}
            print(f"Starting {agent_id} in {thread_id}")
            allies_info = {
                "table_info": "(unit_id, x_coord, y_coord, hp, tier)",
                "fetchall" : GameManager.cur(f"SELECT unit_id, x_coord, y_coord, hp, tier, SUM(ABS({agent_info[0]}-x_coord)+ABS({agent_info[0]}-y_coord)) distance FROM Units u LEFT JOIN Equipment e ON u.equip_id=e.equip_id WHERE team=(SELECT team FROM Units WHERE unit_id='{agent_id}') AND y_coord BETWEEN (SELECT y_coord FROM Units WHERE unit_id='{agent_id}')-5 AND (SELECT y_coord FROM Units WHERE unit_id='{agent_id}')+5 AND x_coord BETWEEN (SELECT x_coord FROM Units WHERE unit_id='{agent_id}')-5 AND (SELECT x_coord FROM Units WHERE unit_id='{agent_id}')+5 AND unit_id<>'{agent_id}' ORDER BY distance"),
                "locked": "message"
            }
            enemies_info = {
                "table_info": "(unit_id, x_coord, y_coord, hp, tier)",
                "fetchall" : GameManager.cur(f"SELECT unit_id, x_coord, y_coord, hp, tier, SUM(ABS({agent_info[0]}-x_coord)+ABS({agent_info[0]}-y_coord)) distance FROM Units u LEFT JOIN Equipment e ON u.equip_id=e.equip_id WHERE team<>(SELECT team FROM Units WHERE unit_id='{agent_id}') AND y_coord BETWEEN (SELECT y_coord FROM Units WHERE unit_id='{agent_id}')-5 AND (SELECT y_coord FROM Units WHERE unit_id='{agent_id}')+5 AND x_coord BETWEEN (SELECT x_coord FROM Units WHERE unit_id='{agent_id}')-5 AND (SELECT x_coord FROM Units WHERE unit_id='{agent_id}')+5 ORDER BY distance"),
                "locked": "decision_id"
            }
            resources_info = GameManager.cur(f"SELECT * FROM Resources WHERE y_coord BETWEEN (SELECT y_coord FROM Units WHERE unit_id='{agent_id}')-5 AND (SELECT y_coord FROM Units WHERE unit_id='{agent_id}')+5 AND x_coord BETWEEN (SELECT x_coord FROM Units WHERE unit_id='{agent_id}')-5 AND (SELECT x_coord FROM Units WHERE unit_id='{agent_id}')+5 ORDER BY type")
            # pdb.set_trace()
            see = lambda info: f"{info['table_info']}:\n{info['fetchall']}" if info['fetchall'][0][0] else f":\nNone nearby, NEVER CALL THE FOLLOWING TOOLS {info['locked']} THIS TURN"
            prompt = f'''Position: ({agent_info[0]}, {agent_info[1]}), HP: {agent_info[2]}, Tier:{agent_info[3]} \nORDERS:{agent_info[4]}\nMessages: {messages}\nWizard's Inventory: {GameManager.inventory_dict}\n
            Game World:\nAllies{see(allies_info)}\n----\nEnemies{see(enemies_info)}\n----\nResources(x_coord, y_coord, type):\n{resources_info}'''
            print()
            print(prompt)
            print()
            try:
                with GameManager.runs.stream(
                    assistant_id=agent_id,
                    thread_id=thread_id,
                    additional_messages=[{"role": "user", "content": prompt}],
                    event_handler=ToolHandler(),
                    parallel_tool_calls=False
                ) as stream:
                    for text in stream.text_deltas:
                        print(text, end="", flush=True)
                    print()
            except Exception as e:
                print(e)
            if not GameManager.decision_dict[agent_id]:
                print(f"Agent {agent_id} did not make a decision")
                GameManager.decision = GameManager.load_default(agent_id, agent_info[0], agent_info[1])
            GameManager.current_agent = None
            GameManager.current_thread = None
            GameManager.current_position = None

    @staticmethod
    def closest_enemy(x_coord, y_coord, enemies):
        closest = None
        closest_dist = float("inf")
        for enemy in enemies:
            if enemy[0] and enemy[1] and enemy[2]:
                dist = abs(enemy[1]-x_coord) + abs(enemy[2]-y_coord)
                if dist < closest_dist:
                    closest = enemy[0]
                    closest_dist = dist
        return closest
    
    @staticmethod
    def decode_traits(traits):
        trait_dict = {"Aggressive":False, "Greedy":False, "Timid":False, "Brave":False, "Charismatic":False, "Industrious":False}
        if traits[0] == "1":
            trait_dict["Aggressive"] = True
        if traits[1] == "1":
            trait_dict["Greedy"] = True
        if traits[2] == "1":
            trait_dict["Timid"] = True
        if traits[3] == "1":
            trait_dict["Brave"] = True
        if traits[4] == "1":
            trait_dict["Charismatic"] = True
        if traits[5] == "1":
            trait_dict["Industrious"] = True
        return trait_dict


    @staticmethod
    def load_default(agent_id, x_coord, y_coord):
        print("LOADING DEFAULT DECISION")
        # pdb.set_trace()
        trait_str = GameManager.assistants.retrieve(agent_id).metadata["traits"]
        traits = GameManager.decode_traits(trait_str)
        enemies = GameManager.cur(f"SELECT unit_id, x_coord, y_coord, hp, tier, SUM(ABS({x_coord}-x_coord)+ABS({y_coord}-y_coord)) distance FROM Units u LEFT JOIN Equipment e ON u.equip_id=e.equip_id WHERE team<>(SELECT team FROM Units WHERE unit_id='{agent_id}') AND y_coord BETWEEN {y_coord}-5 AND {y_coord}+5 AND x_coord BETWEEN {x_coord}-5 AND {x_coord}+5 ORDER BY distance"),
        closest = enemies[0][0][0]
        # pdb.set_trace()
        if closest:
            if traits["Aggressive"] or traits["Brave"]:
                    return {"decision_type":"target", "decision_info":closest}
            elif traits["Timid"] or traits["Charismatic"]:
                    movex = x_coord
                    movey = y_coord
                    if int(enemies[0][0][1]) > x_coord:
                        movex = max(0, x_coord-3)
                    else:
                        movex += 3
                    if int(enemies[0][0][2]) > y_coord:
                        movey = max(0, y_coord-3)
                    else:
                        movey += 3
                    return {"decision_type":"move", "decision_info":f"{movex},{movey}"}
        resource = GameManager.cur(f"SELECT * FROM Resources WHERE y_coord BETWEEN {y_coord}-5 AND {y_coord}+5 AND x_coord BETWEEN {x_coord}-5 AND {x_coord}+5 ORDER BY type"),
        closest = resource[0][0]
        if closest[0]:
            return {"decision_type":"move", "decision_info":f"{closest[0]},{closest[1]}"}
        else:
            return {"decision_type":"move", "decision_info":f"{x_coord+1},{y_coord+1}"}

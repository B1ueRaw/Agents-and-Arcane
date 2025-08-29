from openai import AssistantEventHandler
from openai.types.beta.threads import Text, TextDelta
from typing_extensions import override
from Agents.Manager import GameManager as GM
from ast import literal_eval
import pdb
TURN_LOCK_ERROR = "You have already made a target selection, end your response immediately"
PREREQUISITE_ERROR = "Your decision has not been recorded, call decision_move first"

def decision_move(x_coord, z_coord, unit_id):
    # pdb.set_trace()
    nearby = lambda x, z:abs(x-x_coord) < 5 and abs(z-z_coord) < 5
    unit_info, enemy_info = GM.yield_agents(unit_id)
    if unit_info.decision and  unit_info.decision["decision_type"] == "target":
        return TURN_LOCK_ERROR
    ret = ""
    if nearby(enemy_info.x_coord, enemy_info.z_coord):
        ret += "ENEMY COMMANDER IN RANGE\n"
        unit_info.target_selection[enemy_info.unit_id] = True
    else:
        unit_info.target_selection[enemy_info.unit_id] = False
    
    for enemy in enemy_info.battalion:
        if nearby(enemy["x_coord"], enemy["z_coord"]) < 5:
            ret += f"{enemy}\n"
            unit_info.target_selection[enemy["id"]] = True
        else:
            unit_info.target_selection[enemy["id"]] = False
    unit_info.decision = {"decision_type":"move", "decision_info":f"{x_coord},{z_coord}"}
    return ret if ret != "" else "No available target, only decision_id None will be accepted"
    

def decision_id(target_id, unit_id):
    unit_info, enemy_info = GM.yield_agents(unit_id)
    no_target = (unit_id == "None" or unit_id == "\"None\"")
    if unit_info.target_selection[target_id] or no_target:
        try:
            match unit_info.decision["decision_type"]:
                case "move":
                    unit_info.decision = [unit_info.decision,{"decision_type":"target", "decision_info":target_id}]
                    return f"Decision made to target {target_id}. END YOUR RESPONSE IMMEDIATELY"
                case "target":
                    return TURN_LOCK_ERROR
                case _:
                    return PREREQUISITE_ERROR
        except:
            return PREREQUISITE_ERROR
    return "target not found, call decision_move again to refresh available targets"

class ToolHandlerM(AssistantEventHandler):
    @override
    def on_event(self, event):
        # Retrieve events that are denoted with 'requires_action'
        # since these will have our tool_calls
        if event.event == 'thread.run.requires_action':
            run_id = event.data.id  # Retrieve the run ID from the event data
            self.handle_requires_action(event.data, run_id)
 
    def handle_requires_action(self, data, run_id):
        tool_outputs = []
        for tool in data.required_action.submit_tool_outputs.tool_calls:
            # pdb.set_trace()
            tool_args = literal_eval(tool.function.arguments)
            
            errd = False
            try:
                match tool.function.name:
                    case "decision_id":
                        tool_outputs.append({"tool_call_id": tool.id, "output": decision_id(tool_args["target_id"], data.assistant_id)})
                    case "decision_move":
                        tool_outputs.append({"tool_call_id": tool.id, "output": decision_move(tool_args["x_coord"], tool_args["y_coord"], data.assistant_id)})
                    case _:
                        raise ValueError(f"Unknown tool function: {tool.function.name}")
            except Exception as e:
                print(f"Error in tool call {tool.id}: {str(e)}")
                errd = True
                tool_outputs.append({"tool_call_id": tool.id, "error": str(e)})
            # if not errd:
            # pdb.set_trace()
            print()
            print(f"{tool.function.name}({tool.function.arguments}) -> {tool_outputs[-1]['output']}")
            print()
        # Submit all tool_outputs at the same time
        # pdb.set_trace()
        self.submit_tool_outputs(tool_outputs, run_id)

 
    def submit_tool_outputs(self, tool_outputs, run_id):
        # Use the submit_tool_outputs_stream helper
        with GM.runs.submit_tool_outputs_stream(
            thread_id=self.current_run.thread_id,
            run_id=run_id,
            tool_outputs=tool_outputs,
            event_handler=ToolHandlerM(),
        ) as stream:
            for text in stream.text_deltas:
                print(text, end="", flush=True)
            print()
            # stream.until_done()

m_tool_arr = [
{
    "type": "function",
    "function": {
        "name": "decision_move",
        "description": "DECISION: move towards a selected set of coordinates, if there is a resource at the selected coordinates, harvest it. You can move up to 5 spaces in any direction.",
        "parameters": {
            "type": "object",
            "properties": {
                "x_coord": {
                    "type": "integer",
                    "description": "horizontal position within +/- 5 spaces of the current position"
                },
                "y_coord": {
                    "type": "integer",
                    "description": "vertical position within +/- 5 spaces of the current position"
                }
            },
            "required": ["x_coord", "y_coord"]
        }
    }
},
{
    "type": "function",
    "function": {
        "name": "decision_id",
        "description": "DECISION: engage in combat with an enemy id found in the ENEMIES table (NOT None)",
        "parameters": {
            "type": "object",
            "properties": {
                "target_id": {
                    "type": "string",
                    "description": "the asst_id of the selected enemy, Must match the format \"asst_*\" or be the string literal \"None\""
                }
            },
            "required": ["target_id"]
        }
    }
}]
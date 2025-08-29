from openai import AssistantEventHandler
from openai.types.beta.threads import Text, TextDelta
from typing_extensions import override
from Agents.Manager import GameManager as GM
from ast import literal_eval
from json import dumps
import pdb

curPos = lambda agent_id: GM.agent_dict[agent_id]["Position"]

def message(assistant_id, message, current_agent):
    # pdb.set_trace()
    GM.sent_dict[current_agent][assistant_id] = message
    traits = dumps(GM.assistants.retrieve(current_agent).metadata["traits"])
    insert = "(Charismatic) " if traits["Charismatic"] else ""
    message = f"{insert}{current_agent}: {message}"
    try:
        GM.message_dict[assistant_id]
    except KeyError:
        return "Assistant_id not found. DO NOT ATTEMPT TO MESSAGE THIS UNIT AGAIN THIS TURN"
    if not GM.message_dict[assistant_id]:
        GM.message_dict[assistant_id] = message
    else:
        GM.message_dict[assistant_id] += "\n" + message
    return "Message sent to assistant_id: " + str(assistant_id)

def decision_move(x_coord, y_coord, current_agent):
    # pdb.set_trace()
    GM.decision_dict[current_agent] = {"decision_type":"move", "decision_info":f"{x_coord},{y_coord}"}
    return f"Decision made to move to {x_coord}, {y_coord}."

def decision_id(target_id, current_agent):
    target_list = GM.cur(f"SELECT unit_id, x_coord, y_coord, hp, tier, SUM(ABS({GM.current_position['x']}-x_coord)+ABS({GM.current_position['y']}-y_coord)) distance FROM Units u LEFT JOIN Equipment e ON u.equip_id=e.equip_id WHERE team<>(SELECT team FROM Units WHERE unit_id='{GM.current_agent}') AND y_coord BETWEEN (SELECT y_coord FROM Units WHERE unit_id='{GM.current_agent}')-5 AND (SELECT y_coord FROM Units WHERE unit_id='{GM.current_agent}')+5 AND x_coord BETWEEN (SELECT x_coord FROM Units WHERE unit_id='{GM.current_agent}')-5 AND (SELECT x_coord FROM Units WHERE unit_id='{GM.current_agent}')+5 ORDER BY distance")
    targeted = False
    for target in target_list:
        if target_id == target[0]:
            targeted = True
            break
    if not targeted:
        return "Target not found"
    GM.decision_dict[current_agent] = {"decision_type":"target", "decision_info":target_id}
    return f"Decision made to target {target_id}."

class ToolHandler(AssistantEventHandler):
    # @override
    # def on_text_delta(self, delta: TextDelta, snapshot: Text) -> None:
    #     print(delta.value, end="", flush=True)
    #     print()

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
            decided = False
            try:
                match tool.function.name:
                    case "message":
                        # pdb.set_trace()
                        tool_outputs.append({"tool_call_id": tool.id, "output": message(tool_args["assistant_id"], tool_args["message"], data.assistant_id)})
                    case "decision_id":
                        tool_outputs.append({"tool_call_id": tool.id, "output": decision_id(tool_args["target_id"], data.assistant_id)})
                    case "decision_move":
                        tool_outputs.append({"tool_call_id": tool.id, "output": decision_move(tool_args["x_coord"], tool_args["y_coord"], data.assistant_id)})
                    case _:
                        raise ValueError(f"Unknown tool function: {tool.function.name}")
                decided = tool_outputs[-1]["output"].startswith("Decision made to")
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
        if not decided:
            self.submit_tool_outputs(tool_outputs, run_id)
        else:
            run = GM.runs.cancel(
            thread_id=data.thread_id,
            run_id=run_id
        )

 
    def submit_tool_outputs(self, tool_outputs, run_id):
        # Use the submit_tool_outputs_stream helper
        with GM.runs.submit_tool_outputs_stream(
            thread_id=self.current_run.thread_id,
            run_id=self.current_run.id,
            tool_outputs=tool_outputs,
            event_handler=ToolHandler(),
        ) as stream:
            for text in stream.text_deltas:
                print(text, end="", flush=True)
            print()
            # stream.until_done()

tool_arr = [
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
                    "description": "the asst_id of the selected enemy, Must match the format \"asst_*\""
                }
            },
            "required": ["target_id"]
        }
    }
},
{
    "type": "function",
    "function": {
        "name": "message",
        "description": "Send a message to ONLY an ally id found in the ALLIES table (NOT None)",
        "parameters": {
            "type": "object",
            "properties": {
                "assistant_id": {
                    "type": "string",
                    "description": "the asst_id of the selected ally, Must match the format \"asst_*\""
                },
                "message": {
                    "type": "string",
                    "description": "The message contents"
                }
            },
            "required": ["assistant_id", "message"]
        }
    }
}]
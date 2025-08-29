from openai import AssistantEventHandler
from openai.types.beta.threads import Text, TextDelta
from typing_extensions import override
from Agents.Manager import GameManager as GM
from ast import literal_eval

def spell_request(spell_type, target_id):
    print(f"Spell request made for '{spell_type}' on target '{target_id}'.")
    GM.micro_commander.setspell(spell_type, target_id)
    return f"Spell request made for '{spell_type}' on target '{target_id}'."

# Stub
def battalion_command(order_type):
    print(f"Battalion command issued: {order_type}.")
    return GM.micro_commander.setcommand(order_type)


class MicrobattleHandler(AssistantEventHandler):
    @override
    def on_event(self, event):
        # Handle events requiring tool actions
        if event.event == 'thread.run.requires_action':
            run_id = event.data.id
            self.handle_requires_action(event.data, run_id)

    def handle_requires_action(self, data, run_id):
        tool_outputs = []

        decided = False
        for tool in data.required_action.submit_tool_outputs.tool_calls:
            tool_args = literal_eval(tool.function.arguments)
            try:
                match tool.function.name:
                    case "spell_request":
                        tool_outputs.append({
                            "tool_call_id": tool.id,
                            "output": spell_request(tool_args["spell_type"], tool_args["target_id"])
                        })
                    case "battalion_command":
                        tool_outputs.append({
                            "tool_call_id": tool.id,
                            "output": battalion_command(tool_args["command_type"])
                        })
                    case _:
                        raise ValueError(f"Unknown tool function: {tool.function.name}")
            except Exception as e:
                print(f"Error in tool call {tool.id}: {str(e)}")
                errd = True
                tool_outputs.append({"tool_call_id": tool.id, "error": str(e)})
            
            
            print(f"{tool.function.name}({tool.function.arguments}) -> {tool_outputs[-1]['output']}")
        
        # Submit all tool outputs
        if tool_outputs[-1]["output"]:
            self.submit_tool_outputs(tool_outputs, run_id)
        else:
            GM.runs.cancel(
                thread_id=data.thread_id,
                run_id=run_id
            )
        

    def submit_tool_outputs(self, tool_outputs, run_id):
        with GM.runs.submit_tool_outputs_stream(
            thread_id=self.current_run.thread_id,
            run_id=run_id,
            tool_outputs=tool_outputs,
            event_handler=MicrobattleHandler(),
        ) as stream:
            stream.until_done()

tool_arr = [
    {
        "type": "function",
        "function": {
            "name": "battalion_command",
            "description": "issue a command to one of three strike groups.",
            "parameters": {
                "type": "object",
                "properties": {
                    "command_type": {"type": "string", "description": "One of either attack or defend."}
                },
                "required": ["command_type"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "spell_request",
            "description": "Request a spell to be cast on a specific target during a micro battle.",
            "parameters": {
                "type": "object",
                "properties": {
                    "spell_type": {"type": "string", "description": "Type of spell to cast."},
                    "target_id": {"type": "string", "description": "ID of the target enemy unit."}
                },
                "required": ["spell_type", "target_id"]
            }
        }
    }
]

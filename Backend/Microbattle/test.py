from pydantic import BaseModel
from openai import OpenAI
from os import environ
from dotenv import load_dotenv

try:
    environ.pop("OPENAI_API_KEY", None)
finally:
    load_dotenv()
client = OpenAI()

class MicrobattleEvent(BaseModel):
    x_coord: str
    y_coord: str

completion = client.beta.chat.completions.parse(
    model="gpt-4o-2024-08-06",
    messages=[
        {"role": "system", "content": "You are the decision making protocol for a single unit in a video game. Your position will be provided, you can increment or decrement your x and y positions by one each turn to move towards interesting spaces."},
        {"role": "user", "content": "Your position is (1,1). There is a chest of treasure at (2,2)."},
    ],
    response_format=MicrobattleEvent,
)

event = completion.choices[0].message.parsed

def format_python_object(output):
    indent = 0
    formatted_output = []
    indents = "    "  # You can adjust the number of spaces for each indent level

    for char in str(output):
        if char in "[{(":
            formatted_output.append(char + "\n" + indents * (indent + 1))
            indent += 1
        elif char in "]})":
            indent -= 1
            formatted_output.append("\n" + indents * indent + char)
        elif char == ',':
            formatted_output.append(char + "\n" + indents * indent)
        else:
            formatted_output.append(char)

    return ''.join(formatted_output)

def write_formatted_output_to_file(output, filename="output.txt", append=False):
    formatted_output = format_python_object(output)
    if append:
        with open(filename, 'a') as file:
            file.write("\n\n")
            file.write(formatted_output)
    else:
        with open(filename, 'w') as file:
            file.write(formatted_output)

write_formatted_output_to_file(completion, "output.txt")
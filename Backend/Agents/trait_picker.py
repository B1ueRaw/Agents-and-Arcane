from pydantic import BaseModel
from Agents.Manager import GameManager as GM
from json import loads
import pdb
ALLOWED_TRAITS = {"Aggressive", "Greedy", "Timid", "Brave", "Charismatic", "Industrious"}

class TraitsEvent(BaseModel):
    Aggressive: bool
    Greedy: bool
    Timid: bool
    Brave: bool
    Charismatic: bool
    Industrious: bool

def get_assigned_traits(personality_description):
    completion = GM.chat.completions.parse(
    model="gpt-4o-mini",
    messages=[
            {"role": "system", "content": f'''You are parsing a personality description into a dict of relevant traits. 
             Conduct a similarity search between each trait in the set {ALLOWED_TRAITS} and the contents of the provided description. 
             Insert 'True' for each trait that is a close match, and 'False' for those that are not, only a maximum of three traits may be chosen.'''},
            {"role": "user", "content": personality_description},
        ],
        response_format=TraitsEvent,
    )

    # pdb.set_trace()
    event = loads(completion.choices[0].message.content)
    i = 0
    for key in event:
        if event[key]:
            i += 1
            if i > 3:
                event[key] = False

    return event



'''

The unit is very aggressive and enjoys fighting. It also has a tendency to hoard resources and avoid taking unnecessary risks.

The unit is very strategic and prefers to calculate every move before acting. It avoids rushing into battle and values long-term planning over immediate rewards. This unit is also known for finding innovative solutions to difficult problems and prefers to outsmart its opponents rather than overpower them.

The unit is highly charismatic and excels at inspiring others. It enjoys working in groups and often takes the lead in social interactions. This unit tends to be the one everyone looks to in times of need, with a natural ability to motivate and bring out the best in others.

This unit is fearless and shows no hesitation when diving into the heat of battle. It tends to take unnecessary risks and is often the first to charge into dangerous situations. While effective in combat, this reckless behavior sometimes puts the unit and others in peril.

The unit is extremely hardworking and diligent. It prefers to focus on tasks that require effort and persistence, whether it's gathering resources, building structures, or maintaining equipment. It often takes pride in completing its work to perfection, even if it means working longer hours.

This unit is known for its cunning nature and ability to deceive others. It often prefers to work in the shadows, avoiding direct confrontation when possible. Its tactics involve setting traps, manipulating others, and using stealth to achieve its goals without drawing too much attention.

The unit is timid and prefers to avoid conflict whenever possible. Despite its shy nature, it is resourceful and clever, often finding alternative ways to accomplish tasks without putting itself in harm's way. It excels in support roles, helping others without being on the frontlines.

The unit has a strong desire for wealth and valuable items. It often prioritizes its own gains over the well-being of the group. This unit is always looking for ways to accumulate more resources, even if it means taking them from others.

This unit is brave and will always step up to protect its allies. It never backs down from a fight when someone it cares about is in danger. This protective instinct often leads the unit to be a front-line fighter, willing to sacrifice itself for the safety of the group.

The unit has an insatiable curiosity and a thirst for knowledge. It is always exploring new ideas, learning new skills, and seeking out information. This unit often prefers to avoid combat, focusing instead on acquiring wisdom and sharing knowledge with others.

The unit is emotionally resilient and maintains a calm demeanor in the face of adversity. It rarely shows fear or panic, even in the most difficult situations. This unit can endure prolonged hardships without complaint and is often relied upon for its steadiness in battle.

'''
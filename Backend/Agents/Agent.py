from Agents.Manager import GameManager as GM
from Agents.Tools import tool_arr
from Microbattle.Tools import m_tool_arr
from Microbattle.Sysmsg_Helper import sys_dict, trait_guide, eq_guide
from json import dumps

def generate_agent(backstory, name, traits, type):
    map_sys_msg = f"{sys_dict['header']}\n{sys_dict['map_template']}\n\nEND STEPS\nYOUR BACKSTORY:\n{backstory}\nTRAITS:\n"
    
    for key in traits:
        if traits[key]:
            map_sys_msg += trait_guide[key][0]
    agent = GM.assistants.create(
        instructions=map_sys_msg,
        name=name,
        tools=tool_arr,
        metadata={"traits":dumps(traits), "story":backstory},
        model="gpt-4o-mini"
    )
    return agent.id

def generate_micro(agent_traits, agent_tier, enemy_tier):
    micro_sys_msg = f"{sys_dict['header']}\n{sys_dict['micro_template']}\nTRAITS:\n"

    for key in agent_traits:
        if agent_traits[key]:
            micro_sys_msg += trait_guide[key][1]

    eq_diff = agent_tier - enemy_tier
    eq = ""
    match eq_diff:
        case eq_diff if eq_diff>1:
            eq = eq_guide["winning"]
        case eq_diff if eq_diff<-1:
            eq = eq_guide["losing"]
        case _:
            eq = eq_guide["even"]
    micro_sys_msg += f"RELATIVE EQUIPMENT STRENGTH: {eq}"
    return micro_sys_msg
from openai import OpenAI
client = OpenAI()

if __name__ == "__main__":
    file_ret = client.files.create(
        file=open("mydata.jsonl", "rb"),
        purpose="fine-tune"
    )
    print(file_ret)
    job_ret = client.fine_tuning.jobs.create(
        training_file="@mydata.jsonl", 
        model="gpt-4o-mini-2024-07-18", 
        hyperparameters={
            "n_epochs":2
        }
    )
    print(job_ret)
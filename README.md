<img src="docs/images/image-banner.png" align="middle" width="3000"/>

# PokAImon: Competitive Multi-Agent RL Training
We train Pokemon agents to sumo fight in a Unity environment with Reinforcement Learning. More implementation details can be found in our report paper here. More details on components of this repo can be found in the original cloned README [here](https://github.com/ritabt/PokAImon/blob/main/UNITY_README.md).

This is our final project for Stanford's CS230 and CS231n. The contributors to this repo are Cole Sohn and Rita Tlemcani.

## To use Unity ML

Step 1: Activate venv
```sh
envs\ml_agents\Scripts\activate
```

Step 2: Train
```sh
mlagents-learn config/<name of config file>.yaml --run-id=<give it a name (new name to avoid override)> 
use --force if you want to retrain and overwrite data
```
Note: Config file specifies all the hyperparams, model architecture, optimizer, etc.

Step 3: Press play in Unity

Step 4: Launch TensorBoard to see training
```sh
tensorboard --logdir results --port 6006
```

Step 5: To test a model, move its `.onnx` file (found in `results/run-id/`) to the unity project Assets directory. In Unity, for both agets, drag model into model reference field. Hit play.

## Git Commands
  
Run this command to see what files have been modified since last commit
```sh
git status
```

Run the command below to grab any changes in the repo
```sh
git pull 
```

Run the command below to add specific files/folders
```sh
git add file_name1 file_name2 folder_name
```

or run this command to add the whole directory
```sh
git add . 
```

then commit the files by running the command below. write a small message about what was changed
```sh
git commit -m "my message"
```

then push to see your commits in the repo
```sh
git push 
```

## Important Docs

Trainer File:
https://unity-technologies.github.io/ml-agents/Training-Configuration-File/
  
## Aknowledgments & References
This repo is a cloned and modified version of the [Unity ML Agents repo](https://github.com/Unity-Technologies/ml-agents) and many of the strategies implemented are inspired by [this OpenAI paper](https://arxiv.org/abs/1710.03748). The Pokemon models used are from [this section](https://www.models-resource.com/3ds/pokemonxy/) of the Models Resource website.

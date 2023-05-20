
## To use Unity ML

Step 1: Activate venv
envs\ml_agents\Scripts\activate

Step 2: Cd into directory ml_agents-release_19_branch

Step 3: Train
mlagents-learn config/<name of config file>.yaml --run-id=<give it a name (new name to avoid override)> 
use --force if you want to retrain and overwrite data

Note: Config file specifies all the hyperparams, model architecture, optimizer, etc.

Step 4: Press play in Unity

Step 3: Launch TensorBoard to see training
tensorboard --logdir results --port 6006

Step 5: To test a model, move its .onnx file (found in results/run-id dir) to the unity project Assets directory. In Unity, for both agets, drag model into model reference field. Hit play.

## To add to this repo
Run this command to see what files have been modified since last commit
```sh
git status
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

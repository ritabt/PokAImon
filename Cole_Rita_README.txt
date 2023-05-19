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



Important Docs:

Trainer File:
https://unity-technologies.github.io/ml-agents/Training-Configuration-File/

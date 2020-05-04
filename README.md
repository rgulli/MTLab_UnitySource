# ML_Clean
This repository contains code run a virtual reality experiment using Unity, under continuous control of and communication with the experimental control software MonkeyLogic2. Communication between these programs, behavioural recording devices, and electrophysiological devices is handled by Lab Streaming Layer. 

See the [Wiki page](https://github.com/Doug1983/MTLab_UnitySource/wiki/1.-Installation) for detailed instructions, and a lot of really useful detail on MonkeyLogic, Lab Streaming Layer, Unity, C#, and more. 

This project was originally built by [Guillaume Doucet](https://www.github.com/Doug1983/), and modified by [Roberto Gulli](https://www.github.com/rgulli).

## Requirements
* PC running Windows 10
* [git](https://git-scm.com/download/win)
* Matlab (tested R2019b)
* Unity, downloaded via Unity Hub(https://unity3d.com/get-unity/download). Tested versions include 2019.3.0b3 and 2019.3.0f6. 

## Installation

### Install MonkeyLogic2
* Download MonkeyLogic2 zip folder: https://monkeylogic.nimh.nih.gov/download.html
* Extract to C:\MonkeyLogic\

### Add "Unity task" to your MonkeyLogic2 install
* Navigate to `task` folder in your MonkeyLogic install
  `$ cd /c/MonkeyLogic/task/`
* Clone Unity task for MonkeyLogic2
  `$ git clone --recurse-submodules https://github.com/JMTNeuroLab/MTLab_ML_UnityTask.git `
* In Matlab, add LSL folder to path
  `>> addpath(genpath('C:\MonkeyLogic\task\MTLab_ML_UnityTask\libLSL'))`
* Ensure Lab Streaming Layer folder for the library file `liblsl64.dll` (for 64-bit MATLAB on Windows)

### Clone Unity-MonkeyLogic2 repository from GitHub
* Sign into GitHub, and fork repository from [Guillaume](https://github.com/Doug1983/MTLab_UnitySource) or from [me](https://github.com/rgulli/MTLab_UnitySource)
* Create a folder on your computer to host your local copy of the repository
  `$ cd </desired/path/>`
* Clone repository
  `$ git clone --recurse-submodules https://github.com/<yourGitHubUserName>/MTLab_UnitySource.git .`

## Getting started

To start off, try to replicate a version of the associative memory task used in [Gulli <em>et al.</em> 2020, <em>Nature Neuroscience</em>](https://www.nature.com/articles/s41593-019-0548-3).

* In a terminal window, navigate to the Tasks folder of your local MonkeyLogic-Unity Source repo
  `$ cd <yourLocalSourceRepoPath>/Assets/Tasks/`
* Clone the example task repo called `Temp` to this folder 
  `$ git clone https://github.com/Doug1983/MTLab_UnityExampleTask.git`
* Move all of the files now in `<yourLocalSourceRepoPath>/Assets/Tasks/MTLab_UnityExampleTask/` to `<yourLocalSourceRepoPath>/Assets/Tasks/`
* In Unity Hub, add your local repository as a new project. 
* Open the project using Unity v2019.3.0**

> Note, I got a pop-up asking: 
>> Do you want to upgrade the project to use Asset Database Version 2?
>> 
>> Note: Version 1 is deprecated from 2019.3. If you upgrade to version 2, the project will be re-imported. 
>> 
>> You can always change back to version 1 in the project settings.
> I selected "No". 

>~Note, if Unity project won't open, run : `$ git clean -fxd` in the `~/Assets/Tasks/` folder.~ This seemed to remove a lot of the components necessary to register each scene in the Tasks folder. 






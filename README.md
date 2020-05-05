# ML_Clean
This repository contains code run a virtual reality experiment using Unity, under continuous control of and communication with the experimental control software MonkeyLogic2. Communication between these programs, behavioural recording devices, and electrophysiological devices is handled by Lab Streaming Layer. 

See the [Wiki page](https://github.com/Doug1983/MTLab_UnitySource/wiki/1.-Installation) for detailed instructions, and a lot of really useful detail on MonkeyLogic, Lab Streaming Layer, Unity, C#, and more. 

This project was originally built by [Guillaume Doucet](https://www.github.com/Doug1983/), and modified by [Roberto Gulli](https://www.github.com/rgulli).

## Requirements & Dependencies
* PC running Windows 10
* [git](https://git-scm.com/download/win)
* Matlab (tested R2019b)
* Unity, downloaded via Unity Hub(https://unity3d.com/get-unity/download). Tested versions include 2019.3.0b3 and 2019.3.0f6. 
* [NIMH MonkeyLogic2](https://monkeylogic.nimh.nih.gov/download.html)
* [MonkeyLogic2 task capable of communicating with Unity](https://github.com/JMTNeuroLab/MTLab_ML_UnityTask.git `)
* Unity-MonkeyLogic source code (this repository)
* [Customized Unity task](https://github.com/JMTNeuroLab/MTLab_ML_UnityTask.git), with built-in controllers for communication with MonkeyLogic2, eye tracking, and other experimental equipment
* [Lab Streaming Layer](https://github.com/labstreaminglayer/liblsl-Matlab/releases)
* [Customized EyeLink SDK](https://drive.google.com/drive/folders/1ggGMG3ZsGim3Runcfe7JXZoaC2rzDwap)

## Installation

### Install MonkeyLogic2
* Download MonkeyLogic2 zip folder: https://monkeylogic.nimh.nih.gov/download.html
* Extract to `C:\MonkeyLogic\`

### Add MonkeyLogic2 task for communication with Unity
* Navigate to `task` folder in your MonkeyLogic install
  `$ cd /c/MonkeyLogic/task/`
* Clone Unity task for MonkeyLogic2
  `$ git clone --recurse-submodules https://github.com/JMTNeuroLab/MTLab_ML_UnityTask.git `
* Add Lab Streaming Layer to the task. First, download the latest release of [Lab Streaming Layer](https://github.com/labstreaminglayer/liblsl-Matlab/releases), then extract the contents of the release to `C:\MonkeyLogic\task\MTLab_ML_UnityTask\libLSL\bin\`
* Copy the library file `C:\MonkeyLogic\task\MTLab_ML_UnityTask\libLSL\bin\liblsl-Matlab\bin\liblsl64.dll` to `C:\MonkeyLogic\task\MTLab_ML_UnityTask\libLSL\bin\liblsl64.dll`.
* In Matlab, add LSL folder to path: <br>
  `>> addpath(genpath('C:\MonkeyLogic\task\MTLab_ML_UnityTask\libLSL'))`
> If the installation does not work, run the script `C:\MonkeyLogic\task\MTLab_ML_UnityTask\libLSL\build_mex.m`

### Clone Unity-MonkeyLogic2 source repository from GitHub
* Sign into GitHub, and fork this repository
* Create a folder on your computer to host your local copy of the repository
  `$ cd </desired/path/>`
* Clone repository
  `$ git clone --recurse-submodules https://github.com/<yourGitHubUserName>/MTLab_UnitySource.git .`

### Install the EyeLink SDK
* From the EyeLink SDK folder, run the EyeLink SDK installer (.exe)
	* Choose "Typical" installation when prompted
* Extract `mousesimulator_mar25_2019.zip`
* Run the mouse simulator. Approve the necessary permission requests. 
> Note, this EyeLink SDK is modified, and also [hosted by Guillaume on Google Drive](https://drive.google.com/drive/folders/1ggGMG3ZsGim3Runcfe7JXZoaC2rzDwap) <br>
> *Do not download the latest SDK from the EyeLink support forums*. Our modified version contains a few functions not available in the one provided by SR Research. <br>
> It also uses a specific DLL file that is provided by SR-Research without the source code. This file is included in the MonkeyLogic Unity source (this repository): `Unity-MonkeyLogic2_source\Assets\Scripts\EyeLink\DLLs\interop.SREYELINKLib.dll`.

## Test using the X-Maze associative learning task

To test that your installation is working, you can try to run a replicate of the associative memory task used in [Gulli <em>et al.</em> 2020, <em>Nature Neuroscience</em>](https://www.nature.com/articles/s41593-019-0548-3). This task is built in a virtual environment called the X-Maze. 

### Download the example task
* In a terminal window, navigate to the Tasks folder of your local MonkeyLogic-Unity Source repo
  `$ cd <yourLocalSourceRepoPath>/Assets/Tasks/`
* Clone the example task repo called `Temp` to this folder 
  `$ git clone https://github.com/Doug1983/MTLab_UnityExampleTask.git`
* Move all of the files now in `<yourLocalSourceRepoPath>/Assets/Tasks/MTLab_UnityExampleTask/` to `<yourLocalSourceRepoPath>/Assets/Tasks/`
* In Unity Hub, add your local repository as a new project. 
* Open the project using Unity v2019.3.0**
> Note, you may get a pop-up asking: 
>> Do you want to upgrade the project to use Asset Database Version 2?
>> Note: Version 1 is deprecated from 2019.3. If you upgrade to version 2, the project will be re-imported. 
>> You can always change back to version 1 in the project settings.
> Select "No". 

> ~Note, if Unity project won't open, run : `$ git clean -fxd` in the `<project>/Assets/Tasks/` folder.~ This seemed to remove a lot of the components necessary to register each scene in the Tasks folder. 

### Running the task
#### Unity
 * Launch Unity-MonkeyLogic2 project
 * Load the X-Maze End scene
 * Ensure that there are no errors in the console
 * Hit "Play" (ctrl+P)

#### MonkeyLogic2 
* In Matlab, run: <br>
  `addpath(genpath(C:\MonkeyLogic))`<br>
  `>> monkeylogic()`
  > Note, on first call, you may need to approve some permissions requests. 
* In the MonkeyLogic2 dialog box, load `<path/to/MonkeyLogic2/task/UnityVR.txt>`
* Hit "RUN"

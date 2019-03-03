# Prepare a development environment for Azure IoT Edge

Initially, I wanted to use Visual Studio .NET 2017 to develop and deploy modules to IoT Edge but I quickly realised that Visual Studio Code currently provides better tooling for this.

This section only explains how to set up your development environment with Visual Studio Code as the main tool to develop for Azure IoT Edge. 

What is required:

- .NET Core SDK when modules are being developed in C#
  
- Since IoT Edge modules are containers, [Docker](https://docs.docker.com/install/) must be installed on the development machine.
  
- To have a local development experience for creating, testing and debugging IoT Edge modules, `iotedgehubdev` must be installed.
    Before being able to install `iotedgehubdev` [Python](https://www.python.org/downloads/windows/) must be installed.  When Python is installed, the [`pip`](https://www.makeuseof.com/tag/install-pip-for-python/) command line tool should be available as well.  Run this command to install `iotedgehubdev`:

    ```
    pip install --upgrade iotedgehubdev
    ```

- Install Visual Studio Code if it hasn't been installed yet
- The following extensions for Visual Studio Code must be added:
    - [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools) 
    - [Docker extension](https://marketplace.visualstudio.com/items?itemName=PeterJausovec.vscode-docker)
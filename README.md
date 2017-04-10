# P58_Loss
插件安装说明
---
1. 将PGCreator文件夹整体拷贝到Revit根目录下。例如D:\Revit 2016\
2. 用记事本打开`PGAP.addin`文件，将`Assembly`标签的路径改为对应`PGAP.dll`的绝对路径，并保存更改
3. 将更改后的PGAP.addin文件拷贝至以下路径（取决于Windows版本）：   
   C:\ProgramData\Autodesk\Revit\Addins\2016，或   
   C:\Documents and Settings\All Users\Application Data\Autodesk\Revit\Addins\2016
4. 启动Revit，打开项目文件，点击“附加模块”->“外部工具”->“PGAP”以启动插件
5. 若运行出错，在Revit根目录下的PGCreator文件夹中可找到错误日志

_注意：API版本为2016_

***
# P58_Loss
Install the plugin
---
1. Copy the PGCreator folder (not in this repository) to the root directory of your Revit, e.g. D:\Revit 2016\
2. Open `PGAP.addin` with your favorite text editor. Set the `Assembly` label to the absolute path of `PGAP.dll`, and save the change.
3. Copy the modified `PGAP.addin` to the following path (depending on the version of your Windows):   
   C:\ProgramData\Autodesk\Revit\Addins\2016, or   
   C:\Documents and Settings\All Users\Application Data\Autodesk\Revit\Addins\2016
4. Boot Revit, load the plugin by "Additional module"->"Plugins"->"PGAP"
5. Should there be an error while executing, you can find the error log under path_to_Revit/PGCreator/

_Note: API version 2016_

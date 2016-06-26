# P58_Loss
插件安装说明
---
1. 将PGCreator文件夹整体拷贝到Revit根目录下。例如D:\Revit 2016\
2. 用记事本打开PGAP.addin文件，将<Assembly>标签的路径改为对应PGAP.dll的绝对路径，并保存更改
3. 将更改后的PGAP.addin文件拷贝至以下路径（取决于操作系统）：   
   C:\ProgramData\Autodesk\Revit\Addins\2016，或   
   C:\Documents and Settings\All Users\Application Data\Autodesk\Revit\Addins\2016
4. 启动Revit，打开项目文件，点击“附加模块”->“外部工具”->“PGAP”以启动插件
5. 若运行出错，在Revit根目录下的PGCreator文件夹中可找到错误日志

_注意：API版本为2016_

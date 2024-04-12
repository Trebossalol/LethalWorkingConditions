@echo off

set source="C:\Users\$USERNAME%\source\repos\LethalWorkingConditions\LethalWorkingConditions\bin\Debug\LethalWorkingConditions.dll"

set destination="%appdata%\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\Modding\BepInEx\plugins\LethalWorkingConditions.dll"

copy /Y %source% %destination%

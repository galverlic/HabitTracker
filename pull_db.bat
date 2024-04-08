@echo off
setlocal
set ANDROID_SDK_PATH=C:\Users\galve\AppData\Local\Android\Sdk
set PROJECT_PATH=C:\Users\galve\source\repos\galverlic\HabitTracker
set PACKAGE_NAME=com.mycompany.habittracker
set DB_NAME=HabitTrackerSQLite.db3
set DEVICE_PATH=/data/user/0/%PACKAGE_NAME%/files/%DB_NAME%
set LOCAL_PATH=%PROJECT_PATH%\%DB_NAME%

echo Pulling database from device...
%ANDROID_SDK_PATH%\platform-tools\adb pull %DEVICE_PATH% %LOCAL_PATH%

echo Database pulled successfully.
endlocal

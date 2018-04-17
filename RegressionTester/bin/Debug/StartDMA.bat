cd c:\Program Files (x86)\Skyline Communications\Skyline Taskbar Utility
start SLTaskbarUtility 
timeout 10
start /wait SLTaskbarUtility -start
echo %errorlevel%

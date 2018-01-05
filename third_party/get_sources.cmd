rd /s /q mtsuite
if errorlevel 1 goto error

git clone https://github.com/rpaquay/mtsuite.git
if errorlevel 1 goto error

rd /s /q mtsuite\.git
if errorlevel 1 goto error

echo mtsuite directorty successfully refreshed from github repo.
goto end

:error
echo Error cloning mtsuite repository. Look at console output.

:end
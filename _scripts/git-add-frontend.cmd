@echo off

REM Add client-side folder to staging
git add App/client/business-engine--app/
git add Studio/business-engine--studio/
git commit -m "🔧 Frontend changes committed"

echo.
echo ✅ Frontend committed successfully!
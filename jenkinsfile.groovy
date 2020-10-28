pipeline {
 agent any
 environment {
  dotnet = /usr/bin/dotnet
 }
 stages {
  stage('Checkout') {
   steps {
     git branch: 'master', url: 'https://github.com/stageosu/Kaguya.git'
   }
  }
  stage('Restore Packages') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        bat "dotnet restore"
    }
   }
  }
@REM   stage('Write AppSettings File (API)'){
@REM       steps{
@REM           bat "copy /Y C:\\Users\\admin\\Desktop\\KaguyaCredentials\\appsettings.json \"${env.WORKSPACE}\\KaguyaProjectV2\\appsettings.json\""
@REM       }
@REM   }
  stage('Clean') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        bat "dotnet clean"
        }
      }
    }
  stage('Copy External Dependencies'){
      steps{
          dir("${env.WORKSPACE}/KaguyaProjectV2/ExternalDependencies"){
              bat "copy /Y Discord.Addons.Interactive.dll \"${env.WORKSPACE}\\KaguyaProjectV2\\bin\\Release\\netcoreapp3.1"
              bat "copy /Y oppai.dll \"${env.WORKSPACE}\\KaguyaProjectV2\\bin\\Release\\netcoreapp3.1"
          }
      }
  }
  stage('Build') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        bat "dotnet build -c Release"
    }
   }
  }
  stage('Start Bot + API') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1"){
        bat "dotnet KaguyaProjectV2.dll %Token% %Bot Owner ID% %Log Level% %Default Prefix% %osu! API Key% %Top.gg API Key% %MySQL Username% %MySQL Password% %MySQL Server% %MySQL Schema% %Twitch Client ID% %Twitch Authorization Token% %Danbooru Username% %Danbooru API Key% %Top.gg Webhook Port%"
    }
   }
  }
 }
}
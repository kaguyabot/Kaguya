pipeline {
 agent any
 stages {
  stage('Checkout') {
   steps {
     git branch: 'master', url: 'https://github.com/stageosu/Kaguya.git'
   }
  }
  stage('Restore Packages') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        sh "dotnet restore"
    }
   }
  }
//   stage('Write AppSettings File (API)'){
//       steps{
//           sh "cp /Y C:\\Users\\admin\\Desktop\\KaguyaCredentials\\appsettings.json \"${env.WORKSPACE}\\KaguyaProjectV2\\appsettings.json\""
//       }
//   }
  stage('Clean') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        sh "dotnet clean -c Release"
        }
      }
    }
  stage('Build') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        sh "dotnet build -c Release"
    }
   }
  }
  stage('Copy External Dependencies'){
      steps{
          dir("${env.WORKSPACE}/KaguyaProjectV2/ExternalDependencies"){
              sh "cp -rf Discord.Addons.Interactive.dll ${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1/"
              sh "cp -rf oppai.dll ${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1/"
          }
      }
  }
  stage('Start Bot + API') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1"){
        sh "dotnet KaguyaProjectV2.dll %Token% %BotOwnerID% %LogLevel% %DefaultPrefix% %osu!APIKey% %Top.ggAPIKey% %MySQLUsername% %MySQLPassword% %MySQLServer% %MySQLSchema% %TwitchClientID% %TwitchAuthorizationToken% %DanbooruUsername% %DanbooruAPIKey% %Top.ggWebhookPort%"
    }
   }
  }
 }
}

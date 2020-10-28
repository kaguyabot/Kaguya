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
        sh "dotnet clean"
        }
      }
    }
  stage('Build') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        cp "dotnet build -c Release"
    }
   }
  }
  stage('Copy External Dependencies'){
      steps{
          dir("${env.WORKSPACE}/KaguyaProjectV2/ExternalDependencies"){
              sh "cp /Y Discord.Addons.Interactive.dll ${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1/"
              sh "cp /Y oppai.dll ${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1/"
          }
      }
  }
  stage('Start Bot + API') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1"){
        cp "dotnet KaguyaProjectV2.dll %Token% %Bot Owner ID% %Log Level% %Default Prefix% %osu! API Key% %Top.gg API Key% %MySQL Username% %MySQL Password% %MySQL Server% %MySQL Schema% %Twitch Client ID% %Twitch Authorization Token% %Danbooru Username% %Danbooru API Key% %Top.gg Webhook Port%"
    }
   }
  }
 }
}

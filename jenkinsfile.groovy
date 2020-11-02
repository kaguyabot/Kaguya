def buildNumber = env.BUILD_NUMBER as int
if (buildNumber > 1) milestone(buildNumber - 1)
milestone(buildNumber)

pipeline {
 agent any
 stages {
  stage('Checkout') {
   steps {
     git branch: 'development', url: 'https://github.com/stageosu/Kaguya.git'
   }
  }
  stage('Launch Lavalink') {
    steps {
      sh "java -jar Lavalink.jar"
    }
  }
  stage('Restore Packages') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2"){
        sh "dotnet restore"
    }
   }
  }
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
  stage('Start Bot + API') {
   steps {
    dir("${env.WORKSPACE}/KaguyaProjectV2/bin/Release/netcoreapp3.1"){
        sh "dotnet KaguyaProjectV2.dll ${Token} ${BotOwnerID} ${LogLevel} ${DefaultPrefix} ${osuAPIKey} ${TopggAPIKey} ${MySQLUsername} ${MySQLPassword} ${MySQLServer} ${MySQLSchema} ${TwitchClientID} ${TwitchAuthorizationToken} ${DanbooruUsername} ${DanbooruAPIKey} ${TopggWebhookPort}"
    }
   }
  }
 }
}
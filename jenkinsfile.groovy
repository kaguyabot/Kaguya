pipeline {
 agent any
 stages {
  stage('Cancel Existing Build if Exists') {
  import hudson.model.Result
  import jenkins.model.CauseOfInterruption
  
  //iterate through current project runs
  build.getProject()._getRuns().iterator().each{ run ->
    def exec = run.getExecutor()
    //if the run is not a current build and it has executor (running) then stop it
    if( run!=build && exec!=null ){
        //prepare the cause of interruption
        def cause = { "interrupted by build #${build.getId()}" as String } as CauseOfInterruption 
        exec.interrupt(Result.ABORTED, cause)
      }
    }
  }
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
        sh "dotnet KaguyaProjectV2.dll ${Token} ${BotOwnerID} ${LogLevel} ${DefaultPrefix} ${osuAPIKey} ${TopggAPIKey} ${MySQLUsername} ${MySQLPassword} ${MySQLServer} ${MySQLSchema} ${TwitchClientID} ${TwitchAuthorizationToken} ${DanbooruUsername} ${DanbooruAPIKey} ${TopggWebhookPort}"
    }
   }
  }
 }
}
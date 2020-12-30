pipeline {
    agent any
    stages {
        stage('Git Checkout') {
            steps {
                sh "docker build -t kaguya:${BUILD_NUMBER} ."
                sh "docker tag kaguya:${BUILD_NUMBER} kaguya:latest" 
            }
        } 
    }
}
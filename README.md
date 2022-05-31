## Neuraltafl

 Starter git repo to teach about CICD/TDD

# Intro

 Hey Ryan and friends! Here is a (technically) playable hnefatafl board that we can together use to learn about everything from git practices to tensorflow AI development.
 
 # How to install
 
 Look up how to clone this repo using git commands, or just download the zip for now. You will need [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
 
 Once it's unzipped or cloned, you should be able to just double click runme.bat. Any Windows machine with .NET 6.0 should be supported
 
 # How to play
 
 It is technically playable. I have tested scenarios where the king can be captured and the king can escape, so as long as you play along it works.
 
 Type chess-style commands to play, first selecting a piece (i.e. D1) and then one of the potential moves (D3). Basic validation is in place, but can easily be broken by moving over a piece or selecting a piece that isn't yours. Your job is to fix that.
 
 # How to code
 
 Get your hands on [Visual Studio Code](https://code.visualstudio.com/download) or your favorite IDE and open this folder as a project. I have included the .vscode folder (though it should be in gitignore), just to help aid you in getting a running start on this (vscode can be a picky bastard).
 
 # What should I work on?
 
 We currently have 7 working test cases and 5 red cases. The working cases are movement, captures, and basic validation. The broken cases involve lots of more advanced validation and captures. One case had a really cool recursive solution we could work on together if you'd like.
 
 # What's in the future scope?
 
 Obviously since it's called "neuraltafl" my plan is to do deep learning to make an AI play against itself. I'd also like to take this code into Unity and make an actual UI with clickable pieces. Finally, I'd like to make this a hosted server so that we can play online with our own creation.

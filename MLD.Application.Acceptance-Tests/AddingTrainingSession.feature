Feature: AddingTrainingSession

	In order to register new training session
    As a user
    I want to register new training Session

@tag1
Scenario: RegisteringSession
	Given I want to register new session with name My Training Session
	When GitHash is SomeGitHash
	Then Session should be registered with name My Training Session	
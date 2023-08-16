// Global variables
VAR PlayerName = "John"
VAR ChoseRedPill = false
VAR HealthPoints = 50
VAR Speaker = "Astrid"

// Global Tags
#id:default_convo
#weight:10
#precond: ^Rival, ^Speaker, ^Player
#   ^Rival.relationships.Astrid.tags."rival"
#endprecond


-> start_knot

=== start_knot ===
Hello, {PlayerName}! This is the starting knot! #speaker: {Speaker} // display variable value
Now, we'll go to knot 2! #speaker: {Speaker}
-> knot_2

=== knot_2 ===
Hello from knot 2! #speaker: {Speaker}
Time for a personality test. #speaker: {Speaker}
Red pill or blue pill? #speaker: {Speaker}
*** Red pill
~ChoseRedPill = true // update variable value
-> red_pill
*** Blue pill
~HealthPoints -= 20 // update variable value
-> blue_pill

=== red_pill ===
My god, how brave! #speaker: {Speaker} #thought
-> continue_conversation

=== blue_pill
Bold move, my friend #speaker: {Speaker}
-> continue_conversation

=== continue_conversation
{ HealthPoints < 50: You seem quite weak. I wonder why... #thought }
Alright. You have answered my question. #speaker: {Speaker}
{ ChoseRedPill:
#speaker: {Speaker}
#thought
You chose the red pill. But I'm still not sure I can trust you
}
{not red_pill: -> no_red_pill_comment}
->END

=== no_red_pill_comment ===
#speaker: {Speaker}
#thought
You didn't choose the red pill. I'm not sure I can trust you.
-> END
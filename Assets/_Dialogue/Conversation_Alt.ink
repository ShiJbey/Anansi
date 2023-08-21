# id: conversation_alt
# weight: 1
# where: seen_alt_convo == false

VAR speaker = "Astrid"

-> start_knot

=== start_knot ===

Hello player. # speaker: {speaker}

It seems like you have found your way to my alternative conversation.  # speaker: {speaker}

# db-set: seen_alt_convo to true
Don't get too cocky. It wont happen again.


-> END
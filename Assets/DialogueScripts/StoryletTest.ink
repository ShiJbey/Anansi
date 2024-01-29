INCLUDE ./globals.ink

EXTERNAL SetBackground(background, tags)
EXTERNAL SetSpeakerSprite(speaker, tags)

=== storylet_a
# tags >>  start basic a
# query >>
# ?speaker.relationships.player.stat.Friendship!?friendship
# gte ?friendship 10
# lt ?friendship 30
# end >>

Good to see you again!

-> DONE

=== storylet_b
# tags >> start basic b
# query >>
# ?speaker.relationships.player.stat.Friendship!?val
# eq ?val 0
# end >>

Hi, nice to meet you.

-> DONE

=== storylet_c
# tags >> start basic c
# query >>
# ?speaker.relationships.player.stat.Friendship!?friendship
# gte ?friendship 30
# end >>

Yooooooo! What up??

-> DONE

=== storylet_d
# query >>
# ?speaker.relationships.player.stat.Friendship!?friendship
# lte ?friendship 0
# end >>

Eww. Leave me alone.

-> DONE

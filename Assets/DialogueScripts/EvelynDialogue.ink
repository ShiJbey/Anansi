EXTERNAL IncrementRelationshipStat(a, b, stat, value)
EXTERNAL DispatchSocialEvent(eventId, args)

=== storylet_evelyn_a ===
# ---
# tags: evelyn, convo
# @using speaker as ?speaker
# @query
# ?speaker.relationships.player.stats.Friendship!?friendship
# gte ?friendship 10
# lt ?friendship 30
# @end
# ===

{speaker}: Good to see you again!

-> DONE

=== storylet_evelyn_b ===
# ---
# tags: evelyn, convo
# @using speaker as ?speaker
# @query
# ?speaker.relationships.player.stats.Friendship!?val
# eq ?val 0
# @end
# ===

{speaker}.annoyed: Hi, nice to meet you.

{player}: Hi I'm the player

{QueueStorylet("storylet_sample_conversation")}

-> DONE

=== storylet_evelyn_c(friendship) ===
# ---
# tags: evelyn, convo
# @using speaker as ?speaker
# @query ?friendship
# ?speaker.relationships.player.stats.Friendship!?friendship
# gte ?friendship 30
# @end
# ===

{speaker}: Yooooooo! What up?? (Our friendship level is {friendship}) {IncrementRelationshipStat(speaker, "player", "Friendship", -2)} {DispatchSocialEvent("BecomeBros", "player, {speaker}")}


-> DONE

=== storylet_evelyn_d ===
# ---
# tags: evelyn, convo
# @using speaker as ?speaker
# @query
# ?speaker.relationships.player.stats.Friendship!?friendship
# lt ?friendship 0
# @end
# ===

{speaker}: Eww. Leave me alone.

-> DONE

=== function _storylet_evelyn_d() ===


~ return 7

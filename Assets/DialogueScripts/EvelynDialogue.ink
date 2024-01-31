
=== storylet_evelyn_a ===
# ---
# tags: evelyn, convo
# @using speaker as ?speaker 
# @query 
# ?speaker.relationships.player.stat.Friendship!?friendship
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
# ?speaker.relationships.player.stat.Friendship!?val
# eq ?val 0
# @end
# ===

{speaker}.annoyed: Hi, nice to meet you.

{player}: kdkdkdkdkd

{QueueStorylet("storylet_evelyn_c")}

-> DONE

=== storylet_evelyn_c ===
# ---
# tags: evelyn, convo
# @using speaker as ?speaker 
# @query
# ?speaker.relationships.player.stat.Friendship!?friendship
# gte ?friendship 30
# @end
# ===

{speaker}: Yooooooo! What up??

-> DONE

=== storylet_evelyn_d ===
# ---
# tags: evelyn, convo
# @using speaker as ?speaker 
# @query
# ?speaker.relationships.player.stat.Friendship!?friendship
# lt ?friendship 0
# @end
# ===

{speaker}: Eww. Leave me alone.

-> DONE

=== function _storylet_evelyn_d() ===


~ return 7

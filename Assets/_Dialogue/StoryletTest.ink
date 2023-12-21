=== storylet_a
# set >> Player to ?player
# set >> Speaker to ?npc
# cooldown >> 1
# repeatable >> False
# tags >> start, basic, a
# query >>
# ?player.relationships.?npc.stats.friendship > 0
# end >>

NPC: Welcome to storylet A

-> DONE

=== function _storylet_a

~ return 1

=== storylet_b
# tags >> start, basic, b

NPC: Welcome to storylet B

-> DONE

=== function _storylet_b

~ return 1

=== storylet_c
# tags >> start, basic, c

NPC: Welcome to storylet C

-> DONE

=== function _storylet_c

~ return 1



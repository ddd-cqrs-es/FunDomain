﻿namespace FunDomain

[<AutoOpen>]
module Evolution =
    // Obtains the initialState of the trajectory we're about to load or replay
    let inline initial' () =
        (^S: (static member initial: ^S) ()) 
        
    // Evolves the State a single step along a trajectory
    let inline evolve state event(*'evolution parameter'*) =
        let evolve state = (^S: (static member evolve: ^S -> (^E -> ^S)) state)
        evolve state event
    
    // Evolves the State multiple steps along the trajectory
    let inline evolve' state = List.fold evolve state

    // Evolves the State with the supplied events as the evolution parameter per step
    let inline replay events =
        let bootstrap = initial' ()
        evolve' bootstrap events
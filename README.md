# RaftClassLib


   //$  1. When a leader is active it sends a heart beat within 50ms.
   //  2. When a node receives an AppendEntries from another node, then first node remembers that other node is the current leader.
   //$  3. When a new node is initialized, it should be in follower state.
   //  4. When a follower doesn't get a message for 300ms then it starts an election.
   //  5. When the election time is reset, it is a random value between 150 and 300ms.
   //    * between
   //    * random: call n times and make sure that there are some that are different (other properties of the distribution if you like)
   //  6. When a new election begins, the term is incremented by 1.
   //    * Create a new node, store id in variable.
   //    * wait 300 ms
   //    * reread term(?)
   //    * assert after is greater(by at least 1)
   //  7. When a follower does get an AppendEntries message, it resets the election timer. (i.e.it doesn't start an election even after more than 300ms)
   //  8. Given an election begins, when the candidate gets a majority of votes, it becomes a leader. (think of the easy case; can use two tests for single and multi-node clusters)
   //  9. Given a candidate receives a majority of votes while waiting for unresponsive node, it still becomes a leader.
   // -9. A follower that has not voted and is in a later term than the candidate responds to a RequestForVoteRPC with no. (inverse of 9)
   // 10. A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
   // 11. Given a candidate server that just became a candidate, it votes for itself.
   // 12. Given a candidate, when it receives an AppendEntries message from a node with a later term, then candidate loses and becomes a follower.
   // 13. Given a candidate, when it receives an AppendEntries message from a node with an equal term, then candidate loses and becomes a follower.
   // 14. If a node receives a second request for vote for the same term, it should respond no. (again, separate RPC for response)
   // 15. If a node receives a second request for vote for a future term, it should vote for that node.
   // 16. Given a candidate, when an election timer expires inside of an election, a new election is started.
   //$ 17. When a follower node receives an AppendEntries request, it sends a response.
   // 18. Given a candidate receives an AppendEntries from a previous term, then rejects.
   // 19. When a candidate wins an election, it immediately sends a heart beat.
   //  (testing persistence to disk will be a later assignment)  


   Logging Tests Progress:

1. # when a leader receives a client command the leader sends the log entry in the next appendentries RPC to all nodes
2. #2. when a leader receives a command from the client, it is appended to its log
3. #when a node is new, its log is empty]
4. #when a leader wins an election, it initializes the nextIndex for each follower to the index just after the last one it its log
5. #leaders maintain an "nextIndex" for each follower that is the index of the next log entry the leader will send to that follower
6. #Highest committed index from the leader is included in AppendEntries RPC's
7. #entry index  //I assume this is the next index. the index of the next expected entry. It will be the entry you should expect if you add the list given in the append entry
8. #when a leader receives a majority responses from the clients after a log replication heartbeat, the leader sends a confirmation response to the client
9. #given a leader node, when a log is committed, it applies it to its internal state machine
10. #when a follower receives a valid heartbeat, it increases its commitIndex to match the commit index of the heartbeat
		1. reject the heartbeat if the previous log index / term number does not match your log
11. When sending an AppendEntries RPC, the leader includes the index and term of the entry in its log that immediately precedes the new entries
		1. If the follower does not find an entry in its log with the same index and term, then it refuses the new entries
          1. term must be same or newer
			2. if index is greater, it will be decreased by leader
			3. if index is less, we delete what we have
		2. if a follower rejects the AppendEntries RPC, the leader decrements nextIndex and retries the AppendEntries RPC

12. when a leader sends a heartbeat with a log, but does not receive responses from a majority of nodes, the entry is uncommitted
13. if a leader does not response from a follower, the leader continues to send the log entries in subsequent heartbeats 
14. if a leader cannot commit an entry, it does not send a response to the client
15. if a node receives an appendentries with a logs that are too far in the future from your local state, you should reject the appendentries
16. if a node receives and appendentries with a term and index that do not match, you will reject the appendentry until you find a matching log 

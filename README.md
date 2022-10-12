# Unity Netcode BiteSize Examples
## ![CodeSmile](/Assets/Plugins/CodeSmile/Shared/Textures/CodeSmile-tiny.png) [View Documentation](https://codesmile.gitbook.io/netcode-bitesize-examples/)

With this project I want to learn all about Netcode in detail before I begin applying it to a real project.

My Netcode BiteSize examples cover all possible/relevant workflow combinations of a specific Netcode aspect (eg connection, scenes, spawning). 
They aim to be minimal but very helpful, technically complete solutions.

Made with Unity 2021.3 and Netcode for GameObjects 1.0.
I may port the examples to other networking solutions in the future, to be able to compare them. 
In the meantime, feel free to fork and port it yourself, then send me a pull request if you wish.

## Status

The following examples are complete:

- QuickStart Menu
	- host or join network game using a wizard menu
	- show host's public and local IP (to relay this info to clients)
	- supports domain/host name entry (eg dynamic DNS address)
	- spawns a physics-enabled player prefab (not controllable)
	- player physics simulation disabled for all clients (except host)
- ConnectionManagement
	- Ingame menu for server and client
	- Server can shutdown, kick one or all clients
	- Client can disconnect
	- Disconnect/Shutdown will correctly bring up QuickStart menu
	- All combinations of shutdown/disconnect, then host/join again cause no issues
- SceneManagement
	- Introduces a loader scene, to prevent duplicate NetworkManager instances
	- Upon hosting/joining a game, a scene will be loaded
	- Server menu to load additive scenes (unloads excess scenes automatically)
	- Server can also switch to another scene (unloads additive scenes)
	- All scene events are synchronized with clients, including late joining clients
	- Server logs all OnSceneEvent callbacks to the console
	- Server adding/loading scenes too quickly (previous scene didn't finish loading) logged as warnings
	- Clients can join at any time and should get everything synchronized
	- Additive scenes contain physics-enabled networked objects, only simulated on server-side

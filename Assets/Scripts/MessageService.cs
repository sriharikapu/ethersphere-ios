namespace Mapbox.Unity.Ar
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Networking;
    using MiniJSON;

	public class MessageService : MonoBehaviour {

		/// <summary>
		/// This class handles communication with gamesparks for
		/// removing, loading, and writing new messages. New Message
		/// objects are instantiated here.
		/// </summary>
		private static MessageService _instance;
		public static MessageService Instance { get { return _instance; } }

		public Transform mapRootTransform;

		public GameObject messagePrefabAR;

		void Awake(){
			_instance = this;
			StartCoroutine (LoadAllMessages());
		}

		public void RemoveAllMessages(){
			/*
			new GameSparks.Api.Requests.LogEventRequest ()
				.SetEventKey ("REMOVE_MESSAGES")
				.Send ((response) => {
				if (!response.HasErrors) {
					Debug.Log ("Message Saved To GameSparks...");
				} else {
					Debug.Log ("Error Saving Message Data...");
				}
			});
			*/
		}

        public IEnumerator LoadAllMessages(){

			List<GameObject> messageObjectList = new List<GameObject> ();

			/*
			new GameSparks.Api.Requests.LogEventRequest().SetEventKey("LOAD_MESSAGE").Send((response) => {
				if (!response.HasErrors) {
					Debug.Log("Received Player Data From GameSparks...");
					List<GSData> locations = response.ScriptData.GetGSDataList ("all_Messages");
					for (var e = locations.GetEnumerator (); e.MoveNext ();) {

						GameObject MessageBubble = Instantiate (messagePrefabAR,mapRootTransform);
						Message message = MessageBubble.GetComponent<Message>();

						message.latitude = double.Parse(e.Current.GetString ("messLat"));
						message.longitude = double.Parse(e.Current.GetString ("messLon"));
						message.text = e.Current.GetString ("messText");
						messageObjectList.Add(MessageBubble);
					}
				} else {
					Debug.Log("Error Loading Message Data...");
				}
			});
			*/

			string url = "http://8a90bb4d.ngrok.io/api/messages/all";
			var www = new WWW(url);

			yield return www;

			if (string.IsNullOrEmpty (www.error)) {
				string response = www.text;
				//Deserialize the json response
				IList results = (IList)Json.Deserialize (response);

			

				foreach (IDictionary result in results) {
					GameObject MessageBubble = Instantiate (messagePrefabAR,mapRootTransform);
					Message message = MessageBubble.GetComponent<Message>();

					message.latitude = double.Parse((string)result["lat"]);
					message.longitude = double.Parse((string)result["lng"]);
					message.text = (string)result ["message"];
					messageObjectList.Add(MessageBubble);
				}
			} else {
				Debug.Log("Error loading messages");
				Debug.Log(www.error);
			}

			//pass list of objects to ARmessage provider so they can be placed
			ARMessageProvider.Instance.LoadARMessages (messageObjectList);
		}

		public void SaveMessage(double lat, double lon, string text){
			WWWForm form = new WWWForm();
			var apiUrl = "http://8a90bb4d.ngrok.io/api/messages/new";
			form.AddField("lat", lat.ToString());
			form.AddField("lng", lon.ToString());
			form.AddField("message", text);

            var w = UnityWebRequest.Post(apiUrl, form);
			w.SendWebRequest();
		}
	}
}

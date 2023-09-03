using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeBoxSolutions.AudioSources
{
    class OnlineAudioSources
    {

    }

    class SpotifyAudioSource : OnlineAudioSources
    {
        private readonly SpotifyControls _spotifyControls = new SpotifyControls();
        private string _clientId;
        private string _clientSecret;
        private bool _isActive;

        /// <inheritdoc />
        public Task PlayTrackAsync()
        {
            // Play/pause/next/back controls use the desktop rather than the api
            // because it is quicker and player controls require spotify premium.
            _spotifyControls.Play();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets or sets the spotify client id.
        /// </summary>
        //[AudioSourceSetting("Spotify Client ID")]
        public string ClientId
        {
            get => _clientId;
            set
            {
                if (value == _clientId)
                {
                    return;
                }

                _clientId = value;
                Authorize();
            }
        }

        /// <summary>
        /// Gets or sets the spotify client secret.
        /// </summary>
        //[AudioSourceSetting("Spotify Client secret")]
        public string ClientSecret
        {
            get => _clientSecret;
            set
            {
                if (value == _clientSecret)
                {
                    return;
                }

                _clientSecret = value;
                Authorize();
            }
        }


        private void Authorize()
        {
//            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret) || !_isActive)
//            {
//                return;
//            }

//            //Logger.Debug("Connecting to spotify");
//            var url = "http://localhost:" + LocalPort;
//            _auth?.Stop();
//            _auth = new AuthorizationCodeAuth(ClientId, ClientSecret, url, url, Scope.UserModifyPlaybackState | Scope.UserReadPlaybackState | Scope.UserReadCurrentlyPlaying);

//            if (UseProxy)
//            {
//                _auth.ProxyConfig = _proxyConfig;
//            }

//            if (string.IsNullOrEmpty(RefreshToken))
//            {
//                _auth.Start();
//                _auth.AuthReceived += OnAuthReceived;
//                _auth.OpenBrowser();
//            }
//            else
//            {
//                Logger.Debug("Reusing old refresh token");
//#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//                RefreshAccessToken(); // fire and forget
//#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//            }
        }
    }
}

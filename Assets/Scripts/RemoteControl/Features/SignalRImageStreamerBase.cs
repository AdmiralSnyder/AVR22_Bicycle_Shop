﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Features
{

    public abstract class SignalRImageStreamerBase<TEntityController, TArgs> : SignalRUnityFeatureBase<TEntityController, TArgs>
            where TEntityController : SignalREntityController<TArgs>
    {
        public Camera theCamera;
        public string imageTag;
        private RenderTexture cameraTexture;
        private Texture2D texture2D;

        public int resolutionX;
        public int resolutionY;
        public bool sendImage;

        public override void AwakeVirtual()
        {
            base.AwakeVirtual();
            cameraTexture = new(resolutionX, resolutionY, 32);
            texture2D = new(resolutionX, resolutionY, TextureFormat.RGBA32, false);
        }

        private bool first = true;

        private HubConnection HubConnection;

        public override void Connected(HubConnection hubConnection)
        {
            base.Connected(hubConnection);
            HubConnection = hubConnection;
        }

        private bool measureTime = false;

        public void Update()
        {
            if (measureTime)
            {
                measureTime = false;
                //Debug.Log($"rendering {imageTag} took {Time.deltaTime}s."); // etwa 0.1s für den rendervorgang...
            }
            if ((sendImage && (first || Time.renderedFrameCount % 60 == 0) && HubConnection is not null))
            {
                if (theCamera is { })
                {
                    var pt = theCamera.targetTexture;
                    theCamera.targetTexture = cameraTexture;
                    theCamera.Render();
                    theCamera.targetTexture = pt;
                    
                    var oldActive = RenderTexture.active;
                    RenderTexture.active = cameraTexture;
                    
                    texture2D.ReadPixels(new Rect(0, 0, resolutionX, resolutionY), 0, 0);
                    RenderTexture.active = oldActive;

                    var pngBytes = texture2D.EncodeToPNG();
                    var base64 = Convert.ToBase64String(pngBytes);
                    HubConnection.SendAsync("GameplayImage", imageTag, base64);
                }
                first = false;
                measureTime = true;
            }
        }
    }
}

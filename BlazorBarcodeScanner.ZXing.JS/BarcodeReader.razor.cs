﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBarcodeScanner.ZXing.JS
{
    public partial class BarcodeReader
    {
        [Parameter]
        public string Title { get; set; } = "Scan Barcode from Camera";

        [Parameter]
        public bool StartCameraAutomatically { get; set; } = false;

        [Parameter]
        public bool ShowStart { get; set; } = true;

        [Parameter]
        public bool ShowReset { get; set; } = true;

        [Parameter]
        public bool ShowToggleTorch { get; set; } = true;

        [Parameter]
        public bool ShowResult { get; set; } = true;

        [Parameter]
        public bool ShowVideoDeviceList { get; set; } = true;

        [Parameter]
        public int VideoWidth { get; set; } = 300;

        [Parameter]
        public int VideoHeight { get; set; } = 200;

        [Parameter]
        [Obsolete("This parameter is misspelled and rerouted to `VideoHeight`. Please change your reference as the misspelled property is likely to be removed in future releases.")]
        public int VideoHeigth
        {
            get
            {
                return VideoHeight;
            }
            set
            {
                VideoHeight = value;
            }
        }

        [Parameter]
        public int? StreamHeight { get; set; } = null;

        [Parameter]
        public int? StreamWidth { get; set; } = null;

        [Parameter]
        public EventCallback<BarcodeReceivedEventArgs> OnBarcodeReceived { get; set; }

        public string BarcodeText { get; set; }

        public IEnumerable<VideoInputDevice> VideoInputDevices => _videoInputDevices;

        public string SelectedVideoInputId { get; private set; } = string.Empty;

        private List<VideoInputDevice> _videoInputDevices;
        private BarcodeReaderInterop _backend;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _backend = new BarcodeReaderInterop(JSRuntime);
            await GetVideoInputDevicesAsync();

            BarcodeReaderInterop.BarcodeReceived += ReceivedBarcodeText;
            if (StartCameraAutomatically && _videoInputDevices.Count > 0)
            {
                _backend.SetVideoInputDevice(_videoInputDevices[0].DeviceId);
                StartDecoding();
            }
        }

        private async Task GetVideoInputDevicesAsync()
        {
            _videoInputDevices = await _backend.GetVideoInputDevices("get");
        }

        private void RestartDecoding()
        {
            StopDecoding();
            StartDecoding();
        }

        public void StartDecoding()
        {
            var width = StreamWidth.HasValue ? StreamWidth.Value : 0;
            var height = StreamHeight.HasValue ? StreamHeight.Value : 0;
            _backend.StartDecoding("zxingVideo", width, height);
        }

        public void StopDecoding()
        {
            BarcodeReceivedEventArgs resetBarcodeArgs = new BarcodeReceivedEventArgs();
            resetBarcodeArgs.BarcodeText = "";
            resetBarcodeArgs.TimeReceived = new DateTime();
            ReceivedBarcodeText(resetBarcodeArgs);
            _backend.StopDecoding();
        }

        public void UpdateResolution()
        {
            var width = StreamWidth.HasValue ? StreamWidth.Value : 0;
            var height = StreamHeight.HasValue ? StreamHeight.Value : 0;
            RestartDecoding();
        }

        public void ToggleTorch()
        {
            _backend.ToggleTorch();
        }

        public void TorchOn()
        {
            _backend.SetTorchOn();
        }

        public void TorchOff()
        {
            _backend.SetTorchOff();
        }

        public void SelectVideoInput(VideoInputDevice device)
        {
            ChangeVideoInputSource(device.DeviceId);
        }

        private async void ReceivedBarcodeText(BarcodeReceivedEventArgs args)
        {
            this.BarcodeText = args.BarcodeText;
            await OnBarcodeReceived.InvokeAsync(args);
            StateHasChanged();
        }

        protected void ChangeVideoInputSource(string deviceId)
        {
            _backend.SetVideoInputDevice(deviceId);
            RestartDecoding();
            SelectedVideoInputId = deviceId;
        }

        private void ChangeVideoInputSource(ChangeEventArgs args)
        {
            ChangeVideoInputSource(args.Value.ToString());
        }
    }
}
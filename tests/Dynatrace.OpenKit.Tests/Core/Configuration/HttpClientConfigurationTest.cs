//
// Copyright 2018-2020 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Dynatrace.OpenKit.API;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class HttpClientConfigurationTest
    {
        [Test]
        public void InstanceFromOpenKitConfigTakesOverEndpointUrl()
        {
            // given
            const string endpointUrl = "https://localost:9999/1";
            var openKitConfig = Substitute.For<IOpenKitConfiguration>();
            openKitConfig.EndpointUrl.Returns(endpointUrl);

            var target = HttpClientConfiguration.From(openKitConfig);

            // when
            var obtained = target.BaseUrl;

            // then
            _ = openKitConfig.Received(1).EndpointUrl;
            Assert.That(obtained, Is.EqualTo(endpointUrl));
        }

        [Test]
        public void InstanceFromOpenKitConfigTakesOverApplicationId()
        {
            // given
            const string applicationId = "some cryptic appId";
            var openKitConfig = Substitute.For<IOpenKitConfiguration>();
            openKitConfig.ApplicationId.Returns(applicationId);

            var target = HttpClientConfiguration.From(openKitConfig);

            // when
            var obtained = target.ApplicationId;

            // then
            _ = openKitConfig.Received(1).ApplicationId;
            Assert.That(obtained, Is.EqualTo(applicationId));
        }

        [Test]
        public void InstanceFromOpenKitConfigTakesOverTrustManager()
        {
            // given
            var trustManager = Substitute.For<ISSLTrustManager>();
            var openKitConfig = Substitute.For<IOpenKitConfiguration>();
            openKitConfig.TrustManager.Returns(trustManager);

            var target = HttpClientConfiguration.From(openKitConfig);

            // when
            var obtained = target.SslTrustManager;

            // then
            _ = openKitConfig.Received(1).TrustManager;
            Assert.That(obtained, Is.SameAs(trustManager));
        }

        [Test]
        public void InstanceFromOpenKitConfigTakesOverDefaultServerId()
        {
            // given
            const int defaultServerId = 1;
            var openKitConfig = Substitute.For<IOpenKitConfiguration>();
            openKitConfig.DefaultServerId.Returns(defaultServerId);

            var target = HttpClientConfiguration.From(openKitConfig);

            // when
            var obtained = target.ServerId;

            // then
            _ = openKitConfig.Received(1).DefaultServerId;
            Assert.That(obtained, Is.EqualTo(defaultServerId));
        }

        [Test]
        public void BuilderFromHttpClientConfigTakesOverBaseUrl()
        {
            // given
            const string baseUrl = "https://localhost:9999/1";
            var httpConfig = Substitute.For<IHttpClientConfiguration>();
            httpConfig.BaseUrl.Returns(baseUrl);

            // when
            var target = HttpClientConfiguration.ModifyWith(httpConfig).Build();

            // then
            _ = httpConfig.Received(1).BaseUrl;
            Assert.That(target.BaseUrl, Is.EqualTo(baseUrl));
        }

        [Test]
        public void BuilderFromHttpClientConfigTakesOverApplicationId()
        {
            // given
            const string applicationId = "some cryptic appId";
            var httpConfig = Substitute.For<IHttpClientConfiguration>();
            httpConfig.ApplicationId.Returns(applicationId);

            // when
            var target = HttpClientConfiguration.ModifyWith(httpConfig).Build();

            // then
            _ = httpConfig.Received(1).ApplicationId;
            Assert.That(target.ApplicationId, Is.SameAs(applicationId));
        }

        [Test]
        public void BuilderFromHttpClientConfigTakesOverTrustManager()
        {
            // given
            var trustManager = Substitute.For<ISSLTrustManager>();
            var httpConfig = Substitute.For<IHttpClientConfiguration>();
            httpConfig.SslTrustManager.Returns(trustManager);

            // when
            var target = HttpClientConfiguration.ModifyWith(httpConfig).Build();

            // then
            _ = httpConfig.Received(1).SslTrustManager;
            Assert.That(target.SslTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void BuilderFromHttpClientConfigTakesOverServerId()
        {
            // given
            const int serverId = 1;
            var httpConfig = Substitute.For<IHttpClientConfiguration>();
            httpConfig.ServerId.Returns(serverId);

            // when
            var target = HttpClientConfiguration.ModifyWith(httpConfig).Build();

            // then
            _ = httpConfig.ServerId;
            Assert.That(target.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void EmptyBuilderCreatesEmptyInstance()
        {
            // given
            var target = new HttpClientConfiguration.Builder();

            // when
            var obtained = target.Build();

            // then
            Assert.That(obtained.BaseUrl, Is.Null);
            Assert.That(obtained.ApplicationId, Is.Null);
            Assert.That(obtained.SslTrustManager, Is.Null);
            Assert.That(obtained.ServerId, Is.EqualTo(-1));
        }

        [Test]
        public void BuilderWithBaseUrlPropagatesToInstance()
        {
            // given
            const string baseUrl = "https://localhost:9999/1";
            var target = new HttpClientConfiguration.Builder().WithBaseUrl(baseUrl);

            // when
            var obtained = target.Build();

            // then
            Assert.That(obtained.BaseUrl, Is.EqualTo(baseUrl));
        }

        [Test]
        public void BuilderWithApplicationIdPropagatesToInstance()
        {
            // given
            const string applicationId = "some cryptic appId";
            var target = new HttpClientConfiguration.Builder().WithApplicationId(applicationId);

            // when
            var obtained = target.Build();

            // then
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
        }

        [Test]
        public void BuilderWithTrustManagerPropagatesToInstance()
        {
            // given
            var trustManager = Substitute.For<ISSLTrustManager>();
            var target = new HttpClientConfiguration.Builder().WithTrustManager(trustManager);

            // when
            var obtained = target.Build();

            // then
            Assert.That(obtained.SslTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void BuilderWithServerIdPropagatesToInstance()
        {
            // given
            const int serverId = 1;
            var target = new HttpClientConfiguration.Builder().WithServerId(serverId);

            // when
            var obtained = target.Build();

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }
    }
}
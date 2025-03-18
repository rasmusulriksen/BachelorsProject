// ------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>
// ------------------------------------------------------------------------------

#pragma warning disable SA1200 // Using directives should be placed correctly
using Visma.Ims.Common.Infrastructure;
using Visma.Ims.NotificationAPI;
#pragma warning restore SA1200 // Using directives should be placed correctly

#pragma warning disable SA1002 // Space after colon
return await ProgramBase.DefaultMain(args, ProgramBase.GetConfiguration, ProgramBase.Run<Startup>);
#pragma warning restore SA1002 // Space after colon

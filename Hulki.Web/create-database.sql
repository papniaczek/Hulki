IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AchievementBadges] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IconPath] nvarchar(max) NOT NULL,
        [ConditionType] nvarchar(max) NOT NULL,
        [ConditionValue] int NOT NULL,
        CONSTRAINT [PK_AchievementBadges] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [IsTherapist] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [ConsultationStatuses] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ConsultationStatuses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [FileTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Extension] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_FileTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [ForumCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_ForumCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [GameTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(250) NULL,
        CONSTRAINT [PK_GameTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemRarities] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [HexColor] nvarchar(7) NULL,
        CONSTRAINT [PK_ItemRarities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [MoodTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_MoodTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [AppUserId] nvarchar(max) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [ReportStatuses] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_ReportStatuses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [Surveys] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Surveys] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [TherapyTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_TherapyTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [TherapyGoals] (
        [Id] uniqueidentifier NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Deadline] datetime2 NOT NULL,
        [IsCompleted] bit NOT NULL,
        CONSTRAINT [PK_TherapyGoals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TherapyGoals_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [UserBadges] (
        [Id] uniqueidentifier NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [BadgeId] uniqueidentifier NOT NULL,
        [EarnedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserBadges] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserBadges_AchievementBadges_BadgeId] FOREIGN KEY ([BadgeId]) REFERENCES [AchievementBadges] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserBadges_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [Wallets] (
        [Id] uniqueidentifier NOT NULL,
        [Balance] int NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_Wallets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Wallets_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [Consultations] (
        [Id] uniqueidentifier NOT NULL,
        [PatientId] nvarchar(450) NOT NULL,
        [TherapistId] nvarchar(450) NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [Notes] nvarchar(max) NULL,
        [StatusId] int NOT NULL,
        CONSTRAINT [PK_Consultations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Consultations_AspNetUsers_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Consultations_AspNetUsers_TherapistId] FOREIGN KEY ([TherapistId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Consultations_ConsultationStatuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [ConsultationStatuses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [ForumTopics] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [ForumCategoryId] int NOT NULL,
        CONSTRAINT [PK_ForumTopics] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ForumTopics_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ForumTopics_ForumCategories_ForumCategoryId] FOREIGN KEY ([ForumCategoryId]) REFERENCES [ForumCategories] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [Games] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Cost] int NOT NULL,
        [GameTypeId] int NOT NULL,
        CONSTRAINT [PK_Games] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Games_GameTypes_GameTypeId] FOREIGN KEY ([GameTypeId]) REFERENCES [GameTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [RewardItems] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Price] int NOT NULL,
        [IconPath] nvarchar(300) NULL,
        [ItemRarityId] int NOT NULL,
        CONSTRAINT [PK_RewardItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RewardItems_ItemRarities_ItemRarityId] FOREIGN KEY ([ItemRarityId]) REFERENCES [ItemRarities] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [MoodLogs] (
        [Id] uniqueidentifier NOT NULL,
        [AppUserId] nvarchar(max) NOT NULL,
        [Date] datetime2 NOT NULL,
        [MoodTypeId] int NOT NULL,
        CONSTRAINT [PK_MoodLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MoodLogs_MoodTypes_MoodTypeId] FOREIGN KEY ([MoodTypeId]) REFERENCES [MoodTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [DailyReports] (
        [Id] uniqueidentifier NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ReportStatusId] int NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_DailyReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DailyReports_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DailyReports_ReportStatuses_ReportStatusId] FOREIGN KEY ([ReportStatusId]) REFERENCES [ReportStatuses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [SurveyQuestions] (
        [Id] uniqueidentifier NOT NULL,
        [Text] nvarchar(max) NOT NULL,
        [SurveyId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_SurveyQuestions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SurveyQuestions_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Surveys] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [SurveySubmissions] (
        [Id] uniqueidentifier NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [SurveyId] uniqueidentifier NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SurveySubmissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SurveySubmissions_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SurveySubmissions_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Surveys] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [TherapyGroups] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [MaxParticipants] int NOT NULL,
        [TherapyTypeId] int NOT NULL,
        CONSTRAINT [PK_TherapyGroups] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TherapyGroups_TherapyTypes_TherapyTypeId] FOREIGN KEY ([TherapyTypeId]) REFERENCES [TherapyTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [GoalMilestones] (
        [Id] uniqueidentifier NOT NULL,
        [GoalId] uniqueidentifier NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsCompleted] bit NOT NULL,
        CONSTRAINT [PK_GoalMilestones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoalMilestones_TherapyGoals_GoalId] FOREIGN KEY ([GoalId]) REFERENCES [TherapyGoals] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [PointTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [Amount] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [TransactionDate] datetime2 NOT NULL,
        [WalletId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_PointTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PointTransactions_Wallets_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Wallets] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [VisitDetails] (
        [Id] int NOT NULL IDENTITY,
        [ConsultationId] uniqueidentifier NOT NULL,
        [MedicalHistory] nvarchar(max) NOT NULL,
        [Diagnosis] nvarchar(max) NOT NULL,
        [Recommendations] nvarchar(max) NOT NULL,
        [InternalNotes] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_VisitDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VisitDetails_Consultations_ConsultationId] FOREIGN KEY ([ConsultationId]) REFERENCES [Consultations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [ForumPosts] (
        [Id] uniqueidentifier NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [ForumTopicId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ForumPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ForumPosts_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ForumPosts_ForumTopics_ForumTopicId] FOREIGN KEY ([ForumTopicId]) REFERENCES [ForumTopics] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [GameSessions] (
        [Id] uniqueidentifier NOT NULL,
        [PlayedAt] datetime2 NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_GameSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GameSessions_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GameSessions_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [PatientInventories] (
        [AppUserId] nvarchar(450) NOT NULL,
        [RewardItemId] uniqueidentifier NOT NULL,
        [AcquiredDate] datetime2 NOT NULL,
        CONSTRAINT [PK_PatientInventories] PRIMARY KEY ([AppUserId], [RewardItemId]),
        CONSTRAINT [FK_PatientInventories_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PatientInventories_RewardItems_RewardItemId] FOREIGN KEY ([RewardItemId]) REFERENCES [RewardItems] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [ReportAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileTypeId] int NOT NULL,
        [DailyReportId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ReportAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReportAttachments_DailyReports_DailyReportId] FOREIGN KEY ([DailyReportId]) REFERENCES [DailyReports] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ReportAttachments_FileTypes_FileTypeId] FOREIGN KEY ([FileTypeId]) REFERENCES [FileTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [SurveyAnswers] (
        [Id] uniqueidentifier NOT NULL,
        [SubmissionId] uniqueidentifier NOT NULL,
        [QuestionId] uniqueidentifier NOT NULL,
        [AnswerText] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SurveyAnswers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SurveyAnswers_SurveyQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [SurveyQuestions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SurveyAnswers_SurveySubmissions_SubmissionId] FOREIGN KEY ([SubmissionId]) REFERENCES [SurveySubmissions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [GroupMessages] (
        [Id] int NOT NULL IDENTITY,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [TherapyGroupId] int NOT NULL,
        CONSTRAINT [PK_GroupMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroupMessages_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GroupMessages_TherapyGroups_TherapyGroupId] FOREIGN KEY ([TherapyGroupId]) REFERENCES [TherapyGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [GroupQuests] (
        [Id] int NOT NULL IDENTITY,
        [TherapyGroupId] int NOT NULL,
        [Title] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [QuestType] nvarchar(max) NOT NULL,
        [RewardPoints] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [OptionA] nvarchar(max) NULL,
        [OptionB] nvarchar(max) NULL,
        [OptionC] nvarchar(max) NULL,
        [OptionD] nvarchar(max) NULL,
        [CorrectOption] nvarchar(max) NULL,
        CONSTRAINT [PK_GroupQuests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroupQuests_TherapyGroups_TherapyGroupId] FOREIGN KEY ([TherapyGroupId]) REFERENCES [TherapyGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [GroupResources] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(100) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [TherapyGroupId] int NOT NULL,
        CONSTRAINT [PK_GroupResources] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroupResources_TherapyGroups_TherapyGroupId] FOREIGN KEY ([TherapyGroupId]) REFERENCES [TherapyGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [PatientGroups] (
        [AppUserId] nvarchar(450) NOT NULL,
        [TherapyGroupId] int NOT NULL,
        [JoinedDate] datetime2 NOT NULL,
        [IsApproved] bit NOT NULL,
        CONSTRAINT [PK_PatientGroups] PRIMARY KEY ([AppUserId], [TherapyGroupId]),
        CONSTRAINT [FK_PatientGroups_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PatientGroups_TherapyGroups_TherapyGroupId] FOREIGN KEY ([TherapyGroupId]) REFERENCES [TherapyGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [PrescribedMedication] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Dosage] nvarchar(max) NOT NULL,
        [Duration] nvarchar(max) NOT NULL,
        [VisitDetailsId] int NULL,
        CONSTRAINT [PK_PrescribedMedication] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrescribedMedication_VisitDetails_VisitDetailsId] FOREIGN KEY ([VisitDetailsId]) REFERENCES [VisitDetails] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE TABLE [QuestSubmissions] (
        [Id] int NOT NULL IDENTITY,
        [GroupQuestId] int NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [AnswerText] nvarchar(max) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [IsEvaluated] bit NOT NULL,
        [IsAccepted] bit NOT NULL,
        CONSTRAINT [PK_QuestSubmissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuestSubmissions_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_QuestSubmissions_GroupQuests_GroupQuestId] FOREIGN KEY ([GroupQuestId]) REFERENCES [GroupQuests] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Consultations_PatientId] ON [Consultations] ([PatientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Consultations_StatusId] ON [Consultations] ([StatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Consultations_TherapistId] ON [Consultations] ([TherapistId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DailyReports_AppUserId] ON [DailyReports] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DailyReports_ReportStatusId] ON [DailyReports] ([ReportStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ForumPosts_AppUserId] ON [ForumPosts] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ForumPosts_ForumTopicId] ON [ForumPosts] ([ForumTopicId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ForumTopics_AppUserId] ON [ForumTopics] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ForumTopics_ForumCategoryId] ON [ForumTopics] ([ForumCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Games_GameTypeId] ON [Games] ([GameTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GameSessions_AppUserId] ON [GameSessions] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GameSessions_GameId] ON [GameSessions] ([GameId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GoalMilestones_GoalId] ON [GoalMilestones] ([GoalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GroupMessages_AppUserId] ON [GroupMessages] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GroupMessages_TherapyGroupId] ON [GroupMessages] ([TherapyGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GroupQuests_TherapyGroupId] ON [GroupQuests] ([TherapyGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GroupResources_TherapyGroupId] ON [GroupResources] ([TherapyGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MoodLogs_MoodTypeId] ON [MoodLogs] ([MoodTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PatientGroups_TherapyGroupId] ON [PatientGroups] ([TherapyGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PatientInventories_RewardItemId] ON [PatientInventories] ([RewardItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PointTransactions_WalletId] ON [PointTransactions] ([WalletId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrescribedMedication_VisitDetailsId] ON [PrescribedMedication] ([VisitDetailsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_QuestSubmissions_AppUserId] ON [QuestSubmissions] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_QuestSubmissions_GroupQuestId] ON [QuestSubmissions] ([GroupQuestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReportAttachments_DailyReportId] ON [ReportAttachments] ([DailyReportId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReportAttachments_FileTypeId] ON [ReportAttachments] ([FileTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RewardItems_ItemRarityId] ON [RewardItems] ([ItemRarityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SurveyAnswers_QuestionId] ON [SurveyAnswers] ([QuestionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SurveyAnswers_SubmissionId] ON [SurveyAnswers] ([SubmissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SurveyQuestions_SurveyId] ON [SurveyQuestions] ([SurveyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SurveySubmissions_AppUserId] ON [SurveySubmissions] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SurveySubmissions_SurveyId] ON [SurveySubmissions] ([SurveyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TherapyGoals_AppUserId] ON [TherapyGoals] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TherapyGroups_TherapyTypeId] ON [TherapyGroups] ([TherapyTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserBadges_AppUserId] ON [UserBadges] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserBadges_BadgeId] ON [UserBadges] ([BadgeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VisitDetails_ConsultationId] ON [VisitDetails] ([ConsultationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Wallets_AppUserId] ON [Wallets] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610135200_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260610135200_InitialCreate', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202111_addcustomers'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ForumTopics]') AND [c].[name] = N'Title');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [ForumTopics] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [ForumTopics] ALTER COLUMN [Title] nvarchar(max) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202111_addcustomers'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ForumCategories]') AND [c].[name] = N'Description');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ForumCategories] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [ForumCategories] ALTER COLUMN [Description] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202111_addcustomers'
)
BEGIN
    CREATE TABLE [CustomUsers] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [IsTherapist] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [AspNetUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_CustomUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202111_addcustomers'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CustomUsers_Email] ON [CustomUsers] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202111_addcustomers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615202111_addcustomers', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN

    CREATE OR ALTER TRIGGER dbo.trg_AfterConsultationStatusChange
    ON Consultations
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;

        IF NOT UPDATE(StatusId) RETURN;

        INSERT INTO Notifications (Id, AppUserId, Content, IsRead, CreatedAt)
        SELECT
            NEWID(),
            i.PatientId,
            N'Twoja konsultacja z ' + CONVERT(NVARCHAR, i.StartTime, 120) + N' została oznaczona jako zakończona.',
            0,
            GETDATE()
        FROM inserted  i
        JOIN deleted   d ON i.Id = d.Id
        WHERE i.StatusId = 2
          AND d.StatusId <> 2;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN

    CREATE OR ALTER FUNCTION dbo.fn_GetTotalEarnedPoints(@userId NVARCHAR(450))
    RETURNS INT
    AS
    BEGIN
        DECLARE @total INT;

        SELECT @total = SUM(pt.Amount)
        FROM   PointTransactions pt
        JOIN   Wallets w ON pt.WalletId = w.Id
        WHERE  w.AppUserId = @userId
          AND  pt.Amount > 0;

        RETURN ISNULL(@total, 0);
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN

    CREATE OR ALTER FUNCTION dbo.fn_CountUserBadges(@userId NVARCHAR(450))
    RETURNS INT
    AS
    BEGIN
        DECLARE @cnt INT;

        SELECT @cnt = COUNT(*)
        FROM   UserBadges
        WHERE  AppUserId = @userId;

        RETURN ISNULL(@cnt, 0);
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN

    CREATE OR ALTER PROCEDURE dbo.sp_PurchaseRewardItem
        @userId       NVARCHAR(450),
        @rewardItemId UNIQUEIDENTIFIER
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @price   INT;
        DECLARE @balance INT;
        DECLARE @walletId UNIQUEIDENTIFIER;

        SELECT @price = Price FROM RewardItems WHERE Id = @rewardItemId;
        IF @price IS NULL
        BEGIN
            SELECT 0 AS Success, N'Przedmiot nie istnieje.' AS Message;
            RETURN;
        END;

        SELECT @walletId = Id, @balance = Balance
        FROM   Wallets
        WHERE  AppUserId = @userId;

        IF @balance IS NULL OR @balance < @price
        BEGIN
            SELECT 0 AS Success, N'Niewystarczające saldo.' AS Message;
            RETURN;
        END;

        IF EXISTS (SELECT 1 FROM PatientInventories WHERE AppUserId = @userId AND RewardItemId = @rewardItemId)
        BEGIN
            SELECT 0 AS Success, N'Przedmiot już posiadany.' AS Message;
            RETURN;
        END;

        BEGIN TRANSACTION;

        -- Saldo portfela aktualizowane automatycznie przez trigger
        -- trg_AfterPointTransactionInsert po wstawieniu transakcji poniżej.
        INSERT INTO PointTransactions (Id, Amount, Description, TransactionDate, WalletId)
        VALUES (NEWID(), -@price, N'Zakup przedmiotu ze sklepu', GETDATE(), @walletId);

        INSERT INTO PatientInventories (AppUserId, RewardItemId, AcquiredDate)
        VALUES (@userId, @rewardItemId, GETDATE());

        COMMIT TRANSACTION;

        SELECT 1 AS Success, N'Zakup zakończony sukcesem.' AS Message;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN

    CREATE OR ALTER PROCEDURE dbo.sp_AwardBadge
        @userId  NVARCHAR(450),
        @badgeId UNIQUEIDENTIFIER
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM UserBadges WHERE AppUserId = @userId AND BadgeId = @badgeId)
        BEGIN
            SELECT 0 AS Success, N'Odznaka już przyznana.' AS Message;
            RETURN;
        END;

        INSERT INTO UserBadges (Id, AppUserId, BadgeId, EarnedAt)
        VALUES (NEWID(), @userId, @badgeId, GETDATE());

        INSERT INTO Notifications (Id, AppUserId, Content, IsRead, CreatedAt)
        SELECT NEWID(), @userId, N'Zdobyto nową odznakę: ' + Name, 0, GETDATE()
        FROM   AchievementBadges
        WHERE  Id = @badgeId;

        SELECT 1 AS Success, N'Odznaka przyznana.' AS Message;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN

    CREATE OR ALTER TRIGGER dbo.trg_AfterPointTransactionInsert
    ON PointTransactions
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;

        UPDATE w
        SET    w.Balance = w.Balance + i.Amount
        FROM   Wallets w
        JOIN   inserted i ON w.Id = i.WalletId;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN

    CREATE OR ALTER TRIGGER dbo.trg_PreventNegativeWalletBalance
    ON Wallets
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM inserted WHERE Balance < 0)
        BEGIN
            RAISERROR(N'Saldo portfela nie może być ujemne.', 16, 1);
            ROLLBACK TRANSACTION;
        END;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616122302_AddMoreSqlObjects'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260616122302_AddMoreSqlObjects', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER FUNCTION dbo.fn_GetFullName(@userId NVARCHAR(450))
    RETURNS NVARCHAR(201)
    AS
    BEGIN
        DECLARE @name NVARCHAR(201);
        SELECT @name = FirstName + ' ' + LastName
        FROM   AspNetUsers
        WHERE  Id = @userId;
        RETURN ISNULL(@name, 'Nieznany użytkownik');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER FUNCTION dbo.fn_CountUserConsultations(
        @userId   NVARCHAR(450),
        @statusId INT
    )
    RETURNS INT
    AS
    BEGIN
        DECLARE @cnt INT;
        SELECT @cnt = COUNT(*)
        FROM   Consultations
        WHERE  (PatientId = @userId OR TherapistId = @userId)
          AND  StatusId   = @statusId;
        RETURN ISNULL(@cnt, 0);
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER FUNCTION dbo.fn_AverageWalletBalance()
    RETURNS DECIMAL(18,2)
    AS
    BEGIN
        DECLARE @avg DECIMAL(18,2);
        SELECT @avg = AVG(CAST(Balance AS DECIMAL(18,2))) FROM Wallets;
        RETURN ISNULL(@avg, 0);
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER PROCEDURE dbo.sp_GetUserStats
        @userId NVARCHAR(450)
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            dbo.fn_GetFullName(@userId)                                        AS FullName,
            dbo.fn_CountUserConsultations(@userId, 2)                          AS CompletedConsultations,
            (SELECT COUNT(*) FROM MoodLogs     WHERE AppUserId = @userId)      AS MoodLogCount,
            (SELECT COUNT(*) FROM DailyReports WHERE AppUserId = @userId)      AS DailyReportCount,
            (SELECT COUNT(*) FROM TherapyGoals WHERE AppUserId = @userId
                                                AND IsCompleted = 1)           AS CompletedGoals,
            (SELECT ISNULL(Balance, 0) FROM Wallets WHERE AppUserId = @userId) AS WalletBalance;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER PROCEDURE dbo.sp_AddPatientToGroup
        @userId        NVARCHAR(450),
        @groupId       INT,
        @isApproved    BIT = 0
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (
            SELECT 1 FROM PatientGroups
            WHERE AppUserId = @userId AND TherapyGroupId = @groupId
        )
        BEGIN
            SELECT 0 AS Success, N'Użytkownik już należy do tej grupy.' AS Message;
            RETURN;
        END;

        DECLARE @maxPart INT;
        SELECT @maxPart = MaxParticipants FROM TherapyGroups WHERE Id = @groupId;

        DECLARE @currentCount INT;
        SELECT @currentCount = COUNT(*) FROM PatientGroups WHERE TherapyGroupId = @groupId;

        IF @currentCount >= @maxPart
        BEGIN
            SELECT 0 AS Success, N'Grupa jest pełna.' AS Message;
            RETURN;
        END;

        INSERT INTO PatientGroups (AppUserId, TherapyGroupId, JoinedDate, IsApproved)
        VALUES (@userId, @groupId, GETDATE(), @isApproved);

        SELECT 1 AS Success, N'Użytkownik dodany do grupy.' AS Message;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER PROCEDURE dbo.sp_PurgeOldNotifications
        @olderThanDays INT = 90
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @cutoff DATETIME = DATEADD(DAY, -@olderThanDays, GETDATE());

        DELETE FROM Notifications
        WHERE  IsRead = 0
          AND  CreatedAt < @cutoff;

        SELECT @@ROWCOUNT AS DeletedCount;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER TRIGGER dbo.trg_AfterInsertAspNetUser
    ON AspNetUsers
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO Wallets (Id, AppUserId, Balance)
        SELECT NEWID(), i.Id, 0
        FROM   inserted i
        WHERE  NOT EXISTS (
            SELECT 1 FROM Wallets w WHERE w.AppUserId = i.Id
        );
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN

    CREATE OR ALTER TRIGGER dbo.trg_AfterConsultationStatusChange
    ON Consultations
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;

        IF NOT UPDATE(StatusId) RETURN;

        INSERT INTO Notifications (Id, AppUserId, Content, IsRead, CreatedAt)
        SELECT
            NEWID(),
            i.PatientId,
            N'Twoja konsultacja z ' + CONVERT(NVARCHAR, i.StartTime, 120) + N' została oznaczona jako zakończona.',
            0,
            GETDATE()
        FROM inserted  i
        JOIN deleted   d ON i.Id = d.Id
        WHERE i.StatusId = 2
          AND d.StatusId <> 2;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185059_AddMissingSqlObjects'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260616185059_AddMissingSqlObjects', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185827_FixTherapyGroupDeleteTrigger'
)
BEGIN
    DROP TRIGGER IF EXISTS dbo.trg_PreventDeleteNonEmptyGroup;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185827_FixTherapyGroupDeleteTrigger'
)
BEGIN
    CREATE TABLE [TherapyGroupDeletionLogs] (
        [Id] int NOT NULL IDENTITY,
        [DeletedGroupId] int NOT NULL,
        [DeletedGroupName] nvarchar(100) NOT NULL,
        [DeletedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_TherapyGroupDeletionLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185827_FixTherapyGroupDeleteTrigger'
)
BEGIN

    CREATE OR ALTER TRIGGER dbo.trg_AuditTherapyGroupDelete
    ON TherapyGroups
    AFTER DELETE
    AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO TherapyGroupDeletionLogs (DeletedGroupId, DeletedGroupName, DeletedAt)
        SELECT d.Id, d.Name, GETDATE()
        FROM deleted d;
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616185827_FixTherapyGroupDeleteTrigger'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260616185827_FixTherapyGroupDeleteTrigger', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_admin_login')
        CREATE LOGIN hulki_admin_login WITH PASSWORD = 'Adm1n_P@ssw0rd_2026!', CHECK_POLICY = ON;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_therapist_login')
        CREATE LOGIN hulki_therapist_login WITH PASSWORD = 'Ther@pist_P@ss_2026!', CHECK_POLICY = ON;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_readonly_login')
        CREATE LOGIN hulki_readonly_login WITH PASSWORD = 'Re@dOnly_P@ss_2026!', CHECK_POLICY = ON;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_admin_user')
        CREATE USER hulki_admin_user FOR LOGIN hulki_admin_login;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_therapist_user')
        CREATE USER hulki_therapist_user FOR LOGIN hulki_therapist_login;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_readonly_user')
        CREATE USER hulki_readonly_user FOR LOGIN hulki_readonly_login;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_admin')
        CREATE ROLE role_hulki_admin;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_therapist')
        CREATE ROLE role_hulki_therapist;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_readonly')
        CREATE ROLE role_hulki_readonly;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN
    ALTER ROLE role_hulki_admin     ADD MEMBER hulki_admin_user;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN
    ALTER ROLE role_hulki_therapist ADD MEMBER hulki_therapist_user;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN
    ALTER ROLE role_hulki_readonly  ADD MEMBER hulki_readonly_user;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO role_hulki_admin;
    GRANT EXECUTE ON SCHEMA::dbo TO role_hulki_admin;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    GRANT SELECT, INSERT, UPDATE ON dbo.Consultations      TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.DailyReports       TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.MoodLogs           TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.TherapyGoals       TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.GoalMilestones     TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.TherapyGroups      TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.PatientGroups      TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.GroupMessages      TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.GroupResources     TO role_hulki_therapist;
    GRANT SELECT, INSERT, UPDATE ON dbo.Notifications      TO role_hulki_therapist;
    GRANT SELECT                 ON dbo.AspNetUsers        TO role_hulki_therapist;

    -- Może wywoływać procedury statystyczne i raportowe
    GRANT EXECUTE ON dbo.sp_GetUserStats         TO role_hulki_therapist;
    GRANT EXECUTE ON dbo.sp_AddPatientToGroup    TO role_hulki_therapist;

    -- Jawna blokada – tabele z hasłami i danymi kontowymi pozostają niedostępne
    DENY SELECT, INSERT, UPDATE, DELETE ON dbo.CustomUsers TO role_hulki_therapist;
    DENY SELECT, INSERT, UPDATE, DELETE ON dbo.AspNetUserClaims TO role_hulki_therapist;
    DENY SELECT, INSERT, UPDATE, DELETE ON dbo.AspNetUserLogins TO role_hulki_therapist;
    DENY SELECT, INSERT, UPDATE, DELETE ON dbo.AspNetUserTokens TO role_hulki_therapist;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN

    GRANT SELECT ON dbo.Consultations       TO role_hulki_readonly;
    GRANT SELECT ON dbo.DailyReports        TO role_hulki_readonly;
    GRANT SELECT ON dbo.MoodLogs            TO role_hulki_readonly;
    GRANT SELECT ON dbo.TherapyGoals        TO role_hulki_readonly;
    GRANT SELECT ON dbo.TherapyGroups       TO role_hulki_readonly;
    GRANT SELECT ON dbo.PatientGroups       TO role_hulki_readonly;
    GRANT SELECT ON dbo.Wallets             TO role_hulki_readonly;
    GRANT SELECT ON dbo.PointTransactions   TO role_hulki_readonly;
    GRANT SELECT ON dbo.ForumTopics         TO role_hulki_readonly;
    GRANT SELECT ON dbo.ForumPosts          TO role_hulki_readonly;

    -- Dostęp tylko do funkcji agregujących/statystycznych (nie do surowych haseł)
    GRANT EXECUTE ON dbo.fn_AverageWalletBalance     TO role_hulki_readonly;
    GRANT EXECUTE ON dbo.fn_CountUserConsultations   TO role_hulki_readonly;
    GRANT EXECUTE ON dbo.fn_GetTotalEarnedPoints      TO role_hulki_readonly;
    GRANT EXECUTE ON dbo.fn_CountUserBadges           TO role_hulki_readonly;

    -- Jawna blokada – żadnych modyfikacji, żadnych danych kontowych
    DENY INSERT, UPDATE, DELETE ON SCHEMA::dbo TO role_hulki_readonly;
    DENY SELECT ON dbo.CustomUsers          TO role_hulki_readonly;
    DENY SELECT ON dbo.AspNetUsers          TO role_hulki_readonly;
    DENY SELECT ON dbo.AspNetUserClaims     TO role_hulki_readonly;
    DENY SELECT ON dbo.AspNetUserLogins     TO role_hulki_readonly;
    DENY SELECT ON dbo.AspNetUserTokens     TO role_hulki_readonly;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616195958_AddDatabaseSecurityRoles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260616195958_AddDatabaseSecurityRoles', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    ALTER TABLE dbo.MoodLogs
    ALTER COLUMN AppUserId NVARCHAR(450) NOT NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    DELETE ml FROM dbo.MoodLogs ml
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.AspNetUsers u WHERE u.Id = ml.AppUserId
    );

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MoodLogs_AspNetUsers_AppUserId'
    )
    ALTER TABLE dbo.MoodLogs
    ADD CONSTRAINT FK_MoodLogs_AspNetUsers_AppUserId
        FOREIGN KEY (AppUserId) REFERENCES dbo.AspNetUsers(Id)
        ON DELETE CASCADE;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MoodLogs_AppUserId_Date')
        CREATE NONCLUSTERED INDEX IX_MoodLogs_AppUserId_Date
        ON dbo.MoodLogs (AppUserId, Date DESC);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ForumPosts_ForumTopicId_CreatedAt')
        CREATE NONCLUSTERED INDEX IX_ForumPosts_ForumTopicId_CreatedAt
        ON dbo.ForumPosts (ForumTopicId, CreatedAt);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Consultations_PatientId_StartTime')
        CREATE NONCLUSTERED INDEX IX_Consultations_PatientId_StartTime
        ON dbo.Consultations (PatientId, StartTime DESC);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Consultations_TherapistId_StartTime')
        CREATE NONCLUSTERED INDEX IX_Consultations_TherapistId_StartTime
        ON dbo.Consultations (TherapistId, StartTime DESC);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DailyReports_AppUserId_CreatedAt')
        CREATE NONCLUSTERED INDEX IX_DailyReports_AppUserId_CreatedAt
        ON dbo.DailyReports (AppUserId, CreatedAt DESC);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095345_AddPerformanceIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617095345_AddPerformanceIndexes', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095922_SyncMoodLogModelSnapshot'
)
BEGIN
    CREATE INDEX [IX_MoodLogs_AppUserId] ON [MoodLogs] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617095922_SyncMoodLogModelSnapshot'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617095922_SyncMoodLogModelSnapshot', N'10.0.7');
END;

COMMIT;
GO


CREATE PROCEDURE SP_AddMedia @UserId BIGINT, @MediaId VARCHAR(50), @MediaTitle NVARCHAR(255),
                             @MediaPosterPath VARCHAR(350)
AS
    SET NOCOUNT ON;
-- Check if user already has this media
DECLARE @UserMediaCounter AS TINYINT

SELECT @UserMediaCounter = 1
FROM [UserMedia] AS [UM]
         INNER JOIN [User] AS [U] ON [UM].[UserId] = @UserId
         INNER JOIN [Media] AS [M] ON
    [UM].[MediaId] = @MediaId
    IF @UserMediaCounter IS NULL
        BEGIN
            DECLARE @UserCounter AS TINYINT, @MediaCounter AS TINYINT

            -- Check if user exists
            SELECT @UserCounter = 1 FROM [User] WHERE [Id] = @UserId
            -- Check if media exists
            SELECT @MediaCounter = 1 FROM [Media] WHERE [Id] = @MediaId

            -- Create user if doesn't exist
            IF @UserCounter IS NULL
                BEGIN
                    INSERT INTO [User](Id) VALUES (@UserId)
                END

            -- Create Media if doesn't exist
            IF @MediaCounter IS NULL
                BEGIN
                    INSERT INTO [Media](Id, Title, PosterPath) VALUES (@MediaId, @MediaTitle, @MediaPosterPath)
                END

            -- Insert in conjunction table
            INSERT INTO [UserMedia](UserId, MediaId) VALUES (@UserId, @MediaId)
        END
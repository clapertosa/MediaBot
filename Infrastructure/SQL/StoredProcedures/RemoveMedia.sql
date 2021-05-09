CREATE PROCEDURE SP_RemoveMedia @UserId BIGINT, @MediaId VARCHAR(50)
AS
    SET NOCOUNT ON;
-- Check if user already has this media
DECLARE @UserMediaCounter AS TINYINT

SELECT @UserMediaCounter = 1
FROM [UserMedia] AS [UM]
         INNER JOIN [User] AS [U] ON [UM].[UserId] = @UserId
         INNER JOIN [Media] AS [M] ON
    [UM].[MediaId] = @MediaId
    IF @UserMediaCounter IS NOT NULL
        BEGIN
            DELETE FROM [UserMedia] WHERE UserId = @UserId AND MediaId = @MediaId
        END
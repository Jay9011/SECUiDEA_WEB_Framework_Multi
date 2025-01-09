IF EXISTS(SELECT * FROM sys.objects WHERE name = 'SECU_WEB_LOGIN_SEL' AND type = 'P')
BEGIN
    DROP PROCEDURE [dbo].[SECU_WEB_LOGIN_SEL]
END
GO
CREATE PROCEDURE [dbo].[SECU_WEB_LOGIN_SEL]
		@Type			INT					-- 사용 타입
	,	@ID				VARCHAR(50)			-- 사용자 ID
	,	@OldPassword    VARCHAR(100) = NULL	-- 이전 비밀번호
	,	@UpdateID		INT			 = 0	-- 접근 ID
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
    /*
     * 모든 SELECT 정보는 UserEntity와 맞춰야 한다.
     */

	IF (@Type = 2) -- 로그인 ID 가져오기
    BEGIN
		-- 협력업체인지 확인 PersonTypeID = 2, AuthType = 0
		IF EXISTS (SELECT PID FROM Person WHERE Sabun = @ID AND PersonTypeID = 2)
		BEGIN
			-- 협력업체 사용자 정보
			SELECT	'0' AS [AuthType]
				,	PID AS [Seq]
				,	Sabun AS [ID]
				,	[Password]
				,	[Name]
				,	[Tel]
				,	[OrgID]
				,	[GradeID]
				,	PersonStatusID AS [PersonStatudID]
				,	(SELECT VisitSabunPW FROM SystemSetup) AS [VisitSabunPW]
			FROM	Person 
			WHERE	Sabun = @ID
			;
		END
		-- 임직원인지 확인 PersonTypeID = 0 or 1
		ELSE IF EXISTS (SELECT PID FROM Person WHERE Sabun = @ID AND PersonTypeID IN (0, 1))
		BEGIN
			-- 임직원 사용자 정보
			SELECT	'1' AS [AuthType]
				,	p.PID AS [Seq]
				,	Sabun AS [ID]
				,	[Password]
				,	[Name]
				,	[Tel]
				,	o.[OrgID]
				,	o.[OrgName]
				,	[GradeID]
				,	PersonStatusID AS [PersonStatudID]
				,	(SELECT VisitSabunPW FROM SystemSetup) AS [VisitSabunPW]
				,	CASE 
					WHEN	(SELECT AssignFunc FROM VisitSetup) = 0 THEN 0
					ELSE	AccessAuthority 
				END AS [AccessAuthority]
			FROM	Person p
			INNER JOIN	Org o ON p.OrgID = o.OrgID
			WHERE	Sabun = @ID
			;
		END
		ELSE IF EXISTS (SELECT * FROM EqUser WHERE ID = @ID)
		BEGIN
			-- 관리자 사용자 정보
			SELECT	'2' AS [AuthType]
				,	EqUserID AS [Seq]
				,	[ID]
				,	[Password]
				,	EqUserName AS [Name]
				,	[Tel]
				,	[EqUserLevelID]
				,	(SELECT VisitSabunPW FROM SystemSetup) AS [VisitSabunPW]
				,	[ReservedWord]
				,	[DeleteFlag]
				,	[LoginIP]
				,	[InsertDate]
				,	[UpdateDate]
				,	[UpdateID]
			FROM	EqUser	
			WHERE	ID = @ID
				AND	DeleteFlag = 0
			;
		END
		ELSE
		BEGIN
			-- 유효하지 않은 사용자
			SELECT '-1' AS [AuthType]
		END
	END
END
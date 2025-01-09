IF EXISTS(SELECT * FROM sys.objects WHERE name = 'SECU_WEB_LOGIN_SEL' AND type = 'P')
BEGIN
    DROP PROCEDURE [dbo].[SECU_WEB_LOGIN_SEL]
END
GO
CREATE PROCEDURE [dbo].[SECU_WEB_LOGIN_SEL]
		@Type			INT					-- ��� Ÿ��
	,	@ID				VARCHAR(50)			-- ����� ID
	,	@OldPassword    VARCHAR(100) = NULL	-- ���� ��й�ȣ
	,	@UpdateID		INT			 = 0	-- ���� ID
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
    /*
     * ��� SELECT ������ UserEntity�� ����� �Ѵ�.
     */

	IF (@Type = 2) -- �α��� ID ��������
    BEGIN
		-- ���¾�ü���� Ȯ�� PersonTypeID = 2, AuthType = 0
		IF EXISTS (SELECT PID FROM Person WHERE Sabun = @ID AND PersonTypeID = 2)
		BEGIN
			-- ���¾�ü ����� ����
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
		-- ���������� Ȯ�� PersonTypeID = 0 or 1
		ELSE IF EXISTS (SELECT PID FROM Person WHERE Sabun = @ID AND PersonTypeID IN (0, 1))
		BEGIN
			-- ������ ����� ����
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
			-- ������ ����� ����
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
			-- ��ȿ���� ���� �����
			SELECT '-1' AS [AuthType]
		END
	END
END
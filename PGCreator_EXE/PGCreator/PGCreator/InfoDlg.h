#pragma once
#include "afxwin.h"
#include <queue>

#define MAX_INFO_LENGTH 35


// CInfoDlg �Ի���

class CInfoDlg : public CDialog
{
	DECLARE_DYNAMIC(CInfoDlg)

public:
	CInfoDlg(CWnd* pParent = NULL);   // ��׼���캯��
	virtual ~CInfoDlg();

// �Ի�������
	enum { IDD = IDD_INFO_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV ֧��

	DECLARE_MESSAGE_MAP()
public:
	TCHAR filePath[MAX_PATH];
	CButton m_Bpath;
//	CEdit m_Epath;

	void CharToTchar (const char * _char, TCHAR * tchar);
	void TcharToChar (const TCHAR * tchar, char * _char);
	virtual BOOL OnInitDialog();
	afx_msg void OnClickedButtonPath();
	void InFileInterpret(char* inFile,std::queue<int> &infoQueue);
	void OutputInfo(FILE* fp);
	char* GetFilePath();
	/*
	afx_msg void OnKillfocusEditFile();
	afx_msg void OnKillfocusEditBldg();
	afx_msg void OnKillfocusEditAddress();
	afx_msg void OnKillfocusEditProj();
	*/
	virtual BOOL PreTranslateMessage(MSG* pMsg);
};

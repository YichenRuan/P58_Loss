#pragma once

#define MAX_SETTING_LENGTH 600
#define NUM_SETTING 11

// CNotepadDlg 对话框

class CNotepadDlg : public CDialog
{
	DECLARE_DYNAMIC(CNotepadDlg)

public:
	CNotepadDlg(CWnd* pParent = NULL);   // 标准构造函数
	virtual ~CNotepadDlg();

// 对话框数据
	enum { IDD = IDD_NOTEPAD };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	CEdit* p_ENote;
	virtual BOOL OnInitDialog();
	int setting[NUM_SETTING];
	afx_msg void OnBnClickedOk();
	bool IsLegalInput(char file[]);
	char lastSetting[MAX_SETTING_LENGTH];
	void OutputInfo(FILE* fp);
	void ReadExternalSetting();
	CString m_ENotepad;
};

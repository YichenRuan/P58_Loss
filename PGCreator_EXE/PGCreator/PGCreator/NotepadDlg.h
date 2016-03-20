#pragma once

#define MAX_SETTING_LENGTH 600

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
	char settingDoc[MAX_SETTING_LENGTH];
	afx_msg void OnBnClickedOk();
};

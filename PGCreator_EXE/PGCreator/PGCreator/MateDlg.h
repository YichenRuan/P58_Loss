#pragma once

#define NUM_MATERIAL 9
#define  MAX_MATERIAL_LENGTH 10

// CMateDlg 对话框

class CMateDlg : public CDialog
{
	DECLARE_DYNAMIC(CMateDlg)

public:
	CMateDlg(CWnd* pParent = NULL);   // 标准构造函数
	virtual ~CMateDlg();

// 对话框数据
	enum { IDD = IDD_MATERIAL_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnInitDialog();
	afx_msg void OnClickListMate(NMHDR *pNMHDR, LRESULT *pResult);
	int currItem,currSubItem;
	afx_msg void OnKillfocusEditMate();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	void OutputInfo(FILE* fp);
	CString m_Emate;
	afx_msg void OnClickedButtonDefaultEn();
	afx_msg void OnClickedButtonDefaultZh();
};

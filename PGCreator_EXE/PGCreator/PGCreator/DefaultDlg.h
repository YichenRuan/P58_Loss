#pragma once
#include "afxwin.h"
#include "NotepadDlg.h"


// CDefaultDlg �Ի���

class CDefaultDlg : public CDialog
{
	DECLARE_DYNAMIC(CDefaultDlg)

public:
	CDefaultDlg(CWnd* pParent = NULL);   // ��׼���캯��
	virtual ~CDefaultDlg();

// �Ի�������
	enum { IDD = IDD_DEFAULT_DIALOG};

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV ֧��

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
//	CButton m_Rmf;
//	CButton m_Rsdc;
	virtual BOOL OnInitDialog();
	void OutputInfo(FILE* fp);
	void OutputSetting(FILE* fp);
	CNotepadDlg m_notepadDlg;
	afx_msg void OnClickedButtonNotepad();
	double m_EangleTol;
	double m_EfloorTol;
	afx_msg void OnKillfocusEditAngletol();
	afx_msg void OnKillfocusEditFloortol();
};

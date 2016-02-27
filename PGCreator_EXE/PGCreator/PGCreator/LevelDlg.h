#pragma once
#include "afxwin.h"
#include "afxcmn.h"
#include <queue>

#define MAX_LEVEL_LENGTH 20


// CLevelDlg �Ի���

class CLevelDlg : public CDialog
{
	DECLARE_DYNAMIC(CLevelDlg)

public:
	CLevelDlg(CWnd* pParent = NULL);   // ��׼���캯��
	virtual ~CLevelDlg();

// �Ի�������
	enum { IDD = IDD_LEVEL_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV ֧��

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnInitDialog();
	void InFileInterpret(char* inFile,std::queue<int> &levelQueue);
	void OutputInfo(FILE* fp);
	int* floors;
	afx_msg void OnItemchangedListLevel(NMHDR *pNMHDR, LRESULT *pResult);
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	BOOL ItemChangeOn;
	int num_checked;
	int currRoof;
};
